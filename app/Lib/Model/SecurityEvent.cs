using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace app.Lib.Model
{
    public enum SecurityEventSeverity {
        Unset =0,
        // uppercase used to map exactly like specified in the syllabus of the assignment
        LOW = 1,
        HIGH = 2
    }

    [JsonObject]
    public abstract class SecurityEvent
    {
        [JsonProperty("analysisTime")]
        public DateTime AnalysisTime = DateTime.UtcNow;

        [JsonProperty("eventTime")]
        public DateTime EventTime;

        [JsonProperty("level")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityEventSeverity Level;

        [JsonProperty("type")]
        public abstract string Pattern {get;}

        [JsonProperty("account")]
        public string Account;
    }

    public abstract class SecurityEvent<T> : SecurityEvent
    {
        [JsonProperty("extraProps")]
        public T Payload;
    }

    public class ForeignCountryConnectionEvent : SecurityEvent<ForeignCountryConnectionPayload>
    {
        public override string Pattern => "ForeignCountryConnection";
    }

    public class ForeignCountryConnectionPayload
    {
        public string Country;
    }

    public class DifferentOriginConnectionEvent : SecurityEvent<DifferentOriginConnectionPayload>
    {
        public override string Pattern => "DifferentOriginConnection";
    }

    public class DifferentOriginConnectionPayload
    {
        public Origin[] Origins;
    }
}