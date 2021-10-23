using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.Lib.Model
{

    /*
    {   
        "type":"SuccessfulLogin","timestamp":"2021-10-22T18:02:35.242136-04:00","level":"info",
        "extraProps":{"Country":"Canada","Email":"Brendan.Farrell@humanopen-source.net","IPV4":"228.18.14.8","IPV6":"::ffff:228.18.14.8"}
    }
    */
    
    public struct InboundEvent
    {
        public string Type;
        public DateTime TimeStamp;
        public string Level;
        public JObject ExtraProps;

        public void Reset()
        {
            Type = "";
            TimeStamp = DateTime.MinValue;
            Level = "";
            ExtraProps = null;
        }
    }
}