using System;
using Disruptor;
using app.Lib.EventBus;

namespace app.Lib.EventHandlers
{
    public class InMemoryAnalysisHandler : IEventHandler<EventBufferElement>
    {
        
        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            //todo: analyse events in near real-time
        }
    }
}