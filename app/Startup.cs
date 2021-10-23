using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using app.Lib;
using app.Lib.EventBus;
using app.Lib.EventHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace app
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var inputDisruptor = new InboundDisruptorBackgroundService(
                new UnmarshallingHandler(),
                new InMemoryAnalysisHandler(),
                new PersistanceHandler()
            );
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
