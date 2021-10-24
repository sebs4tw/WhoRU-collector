using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace securityconsumer
{
    //provided for debugging purposes
    class Program
    {
        static void Main(string[] args)
        {
            var hostName = args[0];
            var username = args[1];
            var password = args[2];
            var queueName = args[3];

            var factory = new ConnectionFactory(){ HostName = hostName, UserName = username, Password = password };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("{0}", message);
                };

                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.ReadLine();
            }
        }
    }
}
