using System;
using Disruptor;
using app.Lib.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace app.Lib.EventHandlers
{
    public class InMemoryAnalysisHandler : IEventHandler<EventBufferElement>
    {
        private readonly ILogger logger;

        public InMemoryAnalysisHandler(ILogger<InMemoryAnalysisHandler> logger)
        {
            this.logger = logger;
        }

        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            try{
                //todo: analyse events in near real-time
                if(data.Discard || data.SkipAnalysis)
                {
                    logger.Log(LogLevel.Trace, "Analysis skipped");
                    return;
                }

                logger.Log(LogLevel.Information, data.Country);
            }
            catch(Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Analysis failure.");
            }
        }
    }
}