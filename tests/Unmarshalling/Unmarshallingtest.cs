using NUnit.Framework;
using app.Lib.EventHandlers;
using app;
using app.Lib.Model;
using Microsoft.Extensions.Logging.Abstractions;
using app.Lib.EventBus;
using System;
using System.Linq;

namespace tests.Unmarshalling
{
    public class UnmarshallingTest
    {
        
        private UnmarshallingHandler handler;

        public UnmarshallingTest()
        {

        }

        [SetUp]
        public void Setup()
        {
            var logger = new NullLogger<UnmarshallingHandler>();
            handler = new UnmarshallingHandler(logger);
        }

        private void AnalyzeEvents(EventBufferElement[] events)
        {
            for(int i=0; i< events.Length;i++)
            {
                var last = i == events.Length-1;
                handler.OnEvent(events[i],i, last);
            }
        }

        [Test]
        public void ParsableEventsAreUnmarshalledFullyTest()
        {
            var ev = new EventBufferElement{
                RawMessage = "{\"type\":\"FailedLoginAttempt\",\"timestamp\":\"2021-10-24T11:29:57.6459961-04:00\",\"level\":\"warning\",\"extraProps\":{\"Country\":\"Canada\",\"Email\":\"Leanne.Kuhic@regionalmission-critical.name\",\"IPV4\":\"197.39.72.80\",\"IPV6\":\"::ffff:197.39.72.80\",\"Reason\":\"Attempt timed out\"}}"
            };
               
            handler.OnEvent(ev,0,true);
            Assert.IsFalse(ev.Discard);
            Assert.IsFalse(ev.SkipAnalysis);
            Assert.IsNotNull(ev.UnmarshalledEvent);
            Assert.IsNotNull(ev.UnmarshalledEvent.ExtraProps);
            Assert.AreEqual("FailedLoginAttempt", ev.UnmarshalledEvent.Type);
            Assert.AreEqual("warning",ev.UnmarshalledEvent.Level);
            Assert.IsTrue( Math.Abs((new DateTime(2021,10,24,11,29,57) - ev.UnmarshalledEvent.TimeStamp).Seconds) <= 1);
            Assert.AreEqual("Leanne.Kuhic@regionalmission-critical.name", ev.Email);
            Assert.AreEqual("Canada", ev.Country);
            Assert.AreEqual("197.39.72.80", ev.IPV4);
            Assert.AreEqual("::ffff:197.39.72.80", ev.IPV6);
        }

        [Test]
        public void CorruptedEventsAreDiscardedTest()
        {
            var ev = new EventBufferElement{
                RawMessage = "{\"type0-24T11:29:57.6459961-04:00\",\"level\":\"warning\",\"extraProps\":{\"Country\":\"Canada\",\"Email\":\"Leanne.Kuhic@regionalmission-critical.name\",\"IPV4\":\"197.39.72.80\",\"IPV6\":\"::ffff:197.39.72.80\",\"Reason\":\"Attempt timed out\"}}"
            };
               
            handler.OnEvent(ev,0,true);
            Assert.IsTrue(ev.Discard);
        }

        [Test]
        public void EventsWithoutCompleteOriginAreFlaggedAsNotAnalyzableTest()
        {
            var ev = new EventBufferElement{
                RawMessage = "{\"type\":\"FailedLoginAttempt\",\"timestamp\":\"2021-10-24T11:29:57.6459961-04:00\",\"level\":\"warning\",\"extraProps\":{\"Country\":\"Canada\",\"motorcycle\":\"hell yeah!\"}}"
            };
               
            handler.OnEvent(ev,0,true);
            Assert.IsFalse(ev.Discard);
            Assert.IsTrue(ev.SkipAnalysis);
            Assert.IsNotNull(ev.UnmarshalledEvent);
            Assert.IsNotNull(ev.UnmarshalledEvent.ExtraProps);
            Assert.AreEqual("FailedLoginAttempt", ev.UnmarshalledEvent.Type);
            Assert.AreEqual("warning",ev.UnmarshalledEvent.Level);
            Assert.IsTrue( Math.Abs((new DateTime(2021,10,24,11,29,57) - ev.UnmarshalledEvent.TimeStamp).Seconds) <= 1);
        }

        [Test]
        public void EventsWithMissingHeaderAreDiscardedTest()
        {
            var ev = new EventBufferElement{
                RawMessage = "{\"type\":\"FailedLoginAttempt\",\"level\":\"warning\",\"extraProps\":{\"Country\":\"Canada\",\"Email\":\"Leanne.Kuhic@regionalmission-critical.name\",\"IPV4\":\"197.39.72.80\",\"IPV6\":\"::ffff:197.39.72.80\",\"Reason\":\"Attempt timed out\"}}"
            };
               
            handler.OnEvent(ev,0,true);
            Assert.IsTrue(ev.Discard);
        }
    }
}