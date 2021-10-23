using NUnit.Framework;
using app.Lib;
using System;
using app.Lib.Model;
using System.Collections.Generic;

namespace tests.EventAnalysis
{
    public class MockSecurityBus : ISecurityEventBus
    {
        private readonly List<SecurityEvent> publishedEvents;

        public MockSecurityBus()
        {
            publishedEvents = new List<SecurityEvent>();
        }

        public SecurityEvent[] GetEvents()
        {
            var evs = publishedEvents.ToArray();
            publishedEvents.Clear();
            return evs;
        }

        public void Publish(IEnumerable<SecurityEvent> securityEvents)
        {
            publishedEvents.AddRange(securityEvents);
        }
    }
}