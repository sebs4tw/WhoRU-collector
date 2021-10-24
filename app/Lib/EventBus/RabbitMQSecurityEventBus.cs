using System;
using System.Collections.Generic;
using System.Text;
using app.Lib.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using app.Lib.Configuration;
using Microsoft.Extensions.Logging;

namespace app.Lib.EventBus
{
    public class RabbitMQSecurityEventBus : ISecurityEventBus
    {
        private readonly RabbitMQConfiguration configuration; 
        private readonly ILogger logger;
        public RabbitMQSecurityEventBus(RabbitMQConfiguration configuration, ILogger<RabbitMQSecurityEventBus> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public void Publish(IEnumerable<SecurityEvent> securityEvents)
        {
            var factory = new ConnectionFactory{
                HostName = configuration.HostName, 
                UserName = configuration.UserName, 
                Password = configuration.Password
            };

            try{
                using(var connection = factory.CreateConnection())
                using(var model = connection.CreateModel())
                {
                    foreach(var ev in securityEvents)
                    {
                        var serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ev));
                        model.BasicPublish(configuration.Exchange,configuration.SecurityNotificationQueueName,null,serializedBytes);
                    }
                }
            }
            catch(Exception ex)
            {
                //todo: make notifications more resilient
                logger.Log(LogLevel.Error, ex, "Unable to write notification to channel.");
            }

        }
    }
}