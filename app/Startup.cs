using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using app.Lib;
using app.Lib.EventBus;
using app.Lib.EventHandlers;
using app.Lib.QueryRepositories;
using Disruptor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using app.Lib.Configuration;

namespace app
{
    
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }


        private void ConfigureFromAppSettings(IServiceCollection services)
        {
            var connectionStringProvider = new PostgreSQLConnectionStringProvider
            {
                DatabaseName = Configuration["TimescaleDB:DatabaseName"],
                Port = uint.Parse(Configuration["TimescaleDB:Port"]),
                HostName = Configuration["TimescaleDB:HostName"],
                UserName = Configuration["TimescaleDB:UserName"],
                Password = Configuration["TimescaleDB:Password"]
            };
            if(string.IsNullOrWhiteSpace(connectionStringProvider.UserName)|| string.IsNullOrWhiteSpace(connectionStringProvider.Password))
            {
                throw new Exception("Invalid TimescaleDB configuration.");
            }
            services.AddSingleton<PostgreSQLConnectionStringProvider>(connectionStringProvider);

            var rabbitMQConfig = new RabbitMQConfiguration
            {
                HostName = Configuration["RabbitMQ:HostName"],
                UserName = Configuration["RabbitMQ:UserName"],
                Password = Configuration["RabbitMQ:Password"],
                Exchange = Configuration["RabbitMQ:Exchange"],
                SecurityNotificationQueueName = Configuration["RabbitMQ:SecurityNotificationQueueName"]
            };
            services.AddSingleton<RabbitMQConfiguration>(rabbitMQConfig);

            var differentOriginRuleConfiguration = new DifferentOriginRuleConfiguration
            {
                AnalysisThresholdMs = uint.Parse(Configuration["SecurityNotificationRules:DifferentOrigin:AnalysisThresholdMs"]),
                MaxOriginCount = uint.Parse(Configuration["SecurityNotificationRules:DifferentOrigin:MaxOriginCount"])
            };
            services.AddSingleton<DifferentOriginRuleConfiguration>(differentOriginRuleConfiguration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureFromAppSettings(services);

            services.AddSingleton<LoginEventsQueryRepository>();
            services.AddSingleton<UnmarshallingHandler>();
            services.AddSingleton<CountryAnalysisHandler>();
            services.AddSingleton<DifferentOriginAnalysisHandler>();
            services.AddSingleton<PersistanceHandler>();
            
            //services.AddSingleton<ISecurityEventBus,LoggerSecurityEventBus>();
            services.AddSingleton<ISecurityEventBus, RabbitMQSecurityEventBus>();

            var inputDisruptor = new InboundDisruptorBackgroundService();
            services.AddSingleton<IInboundEventBus>(inputDisruptor);
            services.AddSingleton<InboundDisruptorBackgroundService>(inputDisruptor);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                var inboundBus = endpoints.ServiceProvider.GetService<IInboundEventBus>();

                endpoints.MapPost("/event", async context =>
                {
                    string rawEventMessage;
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        rawEventMessage = await streamReader.ReadToEndAsync();
                    }
                    inboundBus.Publish(rawEventMessage);

                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("");
                });
            });
        }
    }
}
