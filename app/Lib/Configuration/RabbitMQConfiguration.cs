using System;

namespace app.Lib.Configuration
{
    public class RabbitMQConfiguration
    {
        public string HostName {get;set;} = "localhost";
        public string UserName {get;set;} 
        public string Password {get;set;}
        public string SecurityNotificationQueueName {get;set;} = "security-notifications";
        public string Exchange {get;set;} = "";
    }
}