using System;
using System.Collections.Generic;
using app.Lib.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace app.Lib.EventBus
{
    // provided for debugging purposes
    public class LoggerSecurityEventBus : ISecurityEventBus
    {
        private readonly ILogger logger;
        public LoggerSecurityEventBus(ILogger<LoggerSecurityEventBus> logger)
        {
            this.logger = logger;
        }
        
        public void Publish(IEnumerable<SecurityEvent> securityEvents)
        {
            foreach(var ev in securityEvents)
            {
                logger.Log(LogLevel.Warning, JsonConvert.SerializeObject((SecurityEvent)ev));
            }
        }
    }
}