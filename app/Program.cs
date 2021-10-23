using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Lib;
using app.Lib.EventBus;
using app.Lib.EventHandlers;
using Disruptor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services => {
                    services.AddHostedService(sp => {
                        var disruptor = sp.GetService<InboundDisruptorBackgroundService>();
                        disruptor.RegisterHandlers(
                            new IEventHandler<EventBufferElement>[]{ sp.GetService<UnmarshallingHandler>()},
                            new IEventHandler<EventBufferElement>[]{ sp.GetService<InMemoryAnalysisHandler>()},
                            new IEventHandler<EventBufferElement>[]{ sp.GetService<PersistanceHandler>()}
                        );

                        return disruptor;
                    });
                });
    }
}
