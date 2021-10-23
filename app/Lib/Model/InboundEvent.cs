using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.Lib.Model
{
    public struct InboundEvent
    {
        public string Type;
        public DateTime TimeStamp;
        public string Level;
        public JObject ExtraProps;

        public void Reset()
        {
            Type = "";
            TimeStamp = default;
            Level = "";
            ExtraProps = null;
        }
    }
}