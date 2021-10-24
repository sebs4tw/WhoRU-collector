using System;
using Disruptor;
using app.Lib.EventBus;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using app.Lib.Model;
using System.Linq;
using Newtonsoft.Json;
using app.Lib.QueryRepositories;
using app.Lib.Configuration;

namespace app.Lib.EventHandlers
{
    public class DifferentOriginAnalysisHandler : IEventHandler<EventBufferElement>
    {
        private readonly LoginEventsQueryRepository eventsQueryRepository;
        private readonly ISecurityEventBus securityEventBus;
        private readonly ILogger logger;
        private readonly List<SecurityEvent> securityEvents;
        private readonly List<DifferentOriginAnalysisModel> eventsToAnalyze;

        //todo: make configurable in appSettings
        public readonly uint AnalysisThresholdMs = 5 * 60 * 1000;
        public readonly uint MaxOriginCount = 2;

        public DifferentOriginAnalysisHandler(
            DifferentOriginRuleConfiguration configuration,
            LoginEventsQueryRepository eventsQueryRepository, 
            ILogger<DifferentOriginAnalysisHandler> logger, 
            ISecurityEventBus securityEventBus)
        {
            AnalysisThresholdMs = configuration.AnalysisThresholdMs;
            MaxOriginCount = configuration.MaxOriginCount;

            this.eventsQueryRepository = eventsQueryRepository;
            this.securityEventBus = securityEventBus;
            this.logger = logger;
            eventsToAnalyze = new List<DifferentOriginAnalysisModel>();
            securityEvents = new List<SecurityEvent>();
        }

        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            try{
                if(data.Discard || data.SkipAnalysis)
                {
                    return;
                }

                // different origin analysis will be run only once at the end of a batch to increase performance
                var normalizedAccountName = data.Email.ToLower();
                eventsToAnalyze.Add(new DifferentOriginAnalysisModel{
                    Account = normalizedAccountName,
                    TimeStamp = data.UnmarshalledEvent.TimeStamp,
                    Origin = new Origin{
                        Country = data.Country,
                        IPV4 = data.IPV4,
                        IPV6 = data.IPV6
                    }
                });

                if(endOfBatch)
                {
                    AnalyzeEvents();
                    PublishSecurityEvents();
                }
            }
            catch(Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Analysis failure.");
            }
        }

        private void AnalyzeEvents()
        {
            // event context will be read from the database to support some availability edge cases.
            //   performance could be further enhanced by implementing an in-memory cache

            // this algorithm assumes that it is very unlikely that 2 events of the same account
            //  inside the same batch will be in the same analysis threshold.
            //  they will both be analyzed separately should this arise. 
            var orderedEvents = eventsToAnalyze.OrderByDescending(m=>m.TimeStamp).ToArray();

            foreach(var ev in orderedEvents)
            {
                var origins = new HashSet<Tuple<string,string,string>>(){ev.Origin.ToTuple()};
                var lowThreshold = ev.TimeStamp.AddMilliseconds(-AnalysisThresholdMs);
                var highThreshold = ev.TimeStamp.AddMilliseconds(AnalysisThresholdMs);

                // scan for events in queue from the same account
                origins.UnionWith(
                    orderedEvents.Where(e=>
                        e.Account == ev.Account && 
                        e.TimeStamp <= highThreshold && 
                        e.TimeStamp >= lowThreshold
                        )
                        .Select(e => e.Origin.ToTuple())
                );

                // scan from events in database
                origins.UnionWith(
                    eventsQueryRepository.ReadEventsForAnalysis(ev.Account,lowThreshold,highThreshold)
                    .Select(o=> new Origin{
                        Country = o.Country,
                        IPV4 = o.IPV4,
                        IPV6 = o.IPV6
                    }.ToTuple()
                ));

                if(origins.Count() > MaxOriginCount)
                {
                    securityEvents.Add(new DifferentOriginConnectionEvent{
                        Account = ev.Account,
                        EventTime = ev.TimeStamp,
                        Level = SecurityEventSeverity.HIGH,
                        Payload = new DifferentOriginConnectionPayload{
                            Origins = origins.Select(t=> Origin.FromTuple(t)).ToArray() 
                        }
                    });
                }
            }

            eventsToAnalyze.Clear();
        }

        private void PublishSecurityEvents()
        {
            securityEventBus.Publish(securityEvents);
            securityEvents.Clear();
        }
    }
}