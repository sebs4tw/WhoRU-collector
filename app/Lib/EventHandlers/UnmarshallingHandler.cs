using System;
using Disruptor;
using app.Lib.EventBus;
using Newtonsoft.Json;
using app.Lib.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;

namespace app.Lib.EventHandlers
{
    public class UnmarshallingHandler : IEventHandler<EventBufferElement>
    {
        private readonly ILogger logger;

        public UnmarshallingHandler(ILogger<UnmarshallingHandler> logger)
        {
            this.logger = logger;
        }

        public class ExtraPropsPartialMapping
        {
            public string Country;
            public string Email;
            public string IPV4;
            public string IPV6;
        }

        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            try
            {
                logger.Log(LogLevel.Trace, data.RawMessage);

                data.UnmarshalledEvent = JsonConvert.DeserializeObject<InboundEvent>(data.RawMessage);

                if(string.IsNullOrWhiteSpace(data.UnmarshalledEvent.Type) || 
                    data.UnmarshalledEvent.TimeStamp == default || 
                    string.IsNullOrWhiteSpace(data.UnmarshalledEvent.Level))
                {
                    data.Discard = true;
                    return;
                }

                var extraProps = data.UnmarshalledEvent.ExtraProps.ToObject<ExtraPropsPartialMapping>();

                if(extraProps == null ||
                    string.IsNullOrWhiteSpace(extraProps.Country) ||
                    string.IsNullOrWhiteSpace(extraProps.Email) ||
                    string.IsNullOrWhiteSpace(extraProps.IPV4) ||
                    string.IsNullOrWhiteSpace(extraProps.IPV6))
                {
                    data.SkipAnalysis = true;
                    return;
                }
                
                data.Country = extraProps.Country;
                data.Email = extraProps.Email;
                data.IPV4 = extraProps.IPV4;
                data.IPV6 = extraProps.IPV6;
            }
            catch(Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Unmarshalling error");
                data.Discard = true;
            }
        }
    }
}