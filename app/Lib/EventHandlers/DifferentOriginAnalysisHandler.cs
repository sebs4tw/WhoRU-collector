using System;
using Disruptor;
using app.Lib.EventBus;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using app.Lib.Model;
using System.Linq;
using Newtonsoft.Json;

namespace app.Lib.EventHandlers
{
    public class DifferentOriginAnalysisHandler : IEventHandler<EventBufferElement>
    {
        private readonly ISecurityEventBus securityEventBus;
        private readonly ILogger logger;
        private readonly List<SecurityEvent> securityEvents;
        private readonly List<DifferentOriginAnalysisModel> eventsToAnalyze;

        //todo: make configurable in appSettings
        public const int AnalysisThresholdMs = 5 * 60 * 1000;

        public DifferentOriginAnalysisHandler(ILogger<DifferentOriginAnalysisHandler> logger, ISecurityEventBus securityEventBus)
        {
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
                    eventsToAnalyze.Clear();
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
            // read event context from the database. this is necessary to handle events which come out of sequence
            //  and to account for clock drift between instances or service outages

            //todo: read persisted events
        }

        private void PublishSecurityEvents()
        {
            securityEventBus.Publish(securityEvents);
            securityEvents.Clear();
        }
    }
}