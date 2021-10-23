using NUnit.Framework;
using app.Lib.EventHandlers;
using app;
using app.Lib.Model;
using Microsoft.Extensions.Logging.Abstractions;
using app.Lib.EventBus;
using System;
using System.Linq;

namespace tests.EventAnalysis
{
    public class CountryAnalysisTest
    {
        private MockSecurityBus mockSecurityBus;
        private CountryAnalysisHandler analysisHandler;

        public CountryAnalysisTest()
        {
            mockSecurityBus = new MockSecurityBus();
        }

        [SetUp]
        public void Setup()
        {
            var logger = new NullLogger<CountryAnalysisHandler>();
            analysisHandler = new CountryAnalysisHandler(logger, mockSecurityBus);
        }

        private SecurityEvent[] AnalyzeEvents(EventBufferElement[] events)
        {
            for(int i=0; i< events.Length;i++)
            {
                var last = i == events.Length-1;
                analysisHandler.OnEvent(events[i],i, last);
            }
            
            return mockSecurityBus.GetEvents();
        }

        [Test]
        public void SecurityEventsTest()
        {
            var timeBaseline = DateTime.Now;

            var events = new EventBufferElement[]{
                // whitelisted
                new EventBufferElement{
                    UnmarshalledEvent = new InboundEvent{
                        TimeStamp = timeBaseline.AddMinutes(-7)
                    },
                    Country = "Canada",
                    Email = "martin@principalstreamline.biz",
                    IPV4 = "143.1.174.200",
                    IPV6 = "::ffff:143.1.174.200"
                },
                // whitelisted
                new EventBufferElement{
                    UnmarshalledEvent = new InboundEvent{
                        TimeStamp = timeBaseline.AddMinutes(-3)
                    },
                    Country = "USA",
                    Email = "robert@principalstreamline.biz",
                    IPV4 = "143.1.174.192",
                    IPV6 = "::ffff:143.1.174.192"
                },
                // High Risk
                new EventBufferElement{
                    UnmarshalledEvent = new InboundEvent{
                        TimeStamp = timeBaseline.AddMinutes(15).AddSeconds(1)
                    },
                    Country = "Russia",
                    Email = "igorS@principalstreamline.biz",
                    IPV4 = "143.1.174.192",
                    IPV6 = "::ffff:143.1.174.192"
                },
                // Low Risk 
                new EventBufferElement{
                    UnmarshalledEvent = new InboundEvent{
                        TimeStamp = timeBaseline.AddHours(17)
                    },
                    Country = "Motorcycle Land",
                    Email = "newguy@principalstreamline.biz",
                    IPV4 = "143.1.174.192",
                    IPV6 = "::ffff:143.1.174.192"
                }
            };
            var publishedEvents = AnalyzeEvents(events);

            Assert.AreEqual(2, publishedEvents.Length);
            var countryEvents = publishedEvents.OfType<ForeignCountryConnectionEvent>().ToArray();
            Assert.AreEqual(2, countryEvents.Length);
            var highRiskEvent = countryEvents.Where(e=>e.Account == events[2].Email).SingleOrDefault();
            Assert.AreEqual(SecurityEventSeverity.HIGH, highRiskEvent.Level);
            var lowRiskEvent = countryEvents.Where(e=>e.Account == events[3].Email).SingleOrDefault();
            Assert.AreEqual(SecurityEventSeverity.LOW, lowRiskEvent.Level);
        }
    }
}