using System;
using Newtonsoft.Json;

namespace app.Lib.Model
{

    /*
    {   
        "type":"SuccessfulLogin","timestamp":"2021-10-22T18:02:35.242136-04:00","level":"info",
        "extraProps":{"Country":"Canada","Email":"Brendan.Farrell@humanopen-source.net","IPV4":"228.18.14.8","IPV6":"::ffff:228.18.14.8"}
    }
    */
    
    public class InboundEvent
    {
        public string Type;
        public DateTime TimeStamp;
        public string Level;

        // this property will remain in serialized format since it is used as a property bag.
        public string ExtraProps;
    }

    // this model will be used to extract some common parameters present in 'extraProps' which are relevant to the analysis process.
    public class ExtraPropsAnalysisModel
    {
        public string Country;
        public string Email;
        public string IPV4;
        public string IPV6;
    }
}