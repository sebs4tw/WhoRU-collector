using System;
using Disruptor;
using app.Lib.EventBus;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using app.Lib.Model;
using System.Linq;
using Newtonsoft.Json;
using app.Lib.Configuration;

namespace app.Lib.EventHandlers
{
    public class CountryAnalysisHandler : IEventHandler<EventBufferElement>
    {
        private readonly ISecurityEventBus securityEventBus;
        private readonly ILogger logger;
        private readonly List<SecurityEvent> securityEvents;
        private readonly CountryOriginRuleConfiguration ruleConfiguration;

        public CountryAnalysisHandler(CountryOriginRuleConfiguration ruleConfig,ILogger<CountryAnalysisHandler> logger, ISecurityEventBus securityEventBus)
        {
            this.ruleConfiguration = ruleConfig;
            this.securityEventBus = securityEventBus;
            this.logger = logger;
            securityEvents = new List<SecurityEvent>();
        }

        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            try{
                if(data.Discard || data.SkipAnalysis)
                {
                    return;
                }

                // country analysis can be performed in real-time
                if(!ruleConfiguration.Trusted.Contains(data.Country))
                {
                    var severity = ruleConfiguration.HighRisk.Contains(data.Country) ? SecurityEventSeverity.HIGH : SecurityEventSeverity.LOW;
                    securityEvents.Add(new ForeignCountryConnectionEvent{
                        EventTime = data.UnmarshalledEvent.TimeStamp,
                        Account = data.Email,
                        Level = severity,
                        Payload = new ForeignCountryConnectionPayload{
                            Country = data.Country
                        }
                    });
                }

                // publish security events at the end of a batch
                if(endOfBatch)
                {
                    PublishSecurityEvents();
                }
            }
            catch(Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Analysis failure.");
            }
        }

        private void PublishSecurityEvents()
        {
            securityEventBus.Publish(securityEvents);
            securityEvents.Clear();
        }
    }
}