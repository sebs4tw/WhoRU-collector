using System;
using app.Lib.Model;

namespace app.Lib.EventBus
{
    public class EventBufferElement 
    {
        public bool Discard;
        public bool SkipAnalysis;
        public string RawMessage;
        public InboundEvent UnmarshalledEvent = new InboundEvent();
        public string Type;
        public DateTime TimeStamp;
        public string Country;
        public string Email;
        public string IPV4;
        public string IPV6;

        public void Reset(string message)
        {
            RawMessage = message;
            SkipAnalysis = false;
            UnmarshalledEvent.Reset();
            Discard = false;
            Type = "";
            TimeStamp = DateTime.MinValue;
            Country = "";
            Email = "";
            IPV4 = "";
            IPV6 = "";
        }
    }
}