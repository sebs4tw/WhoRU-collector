using System;
using Disruptor;
using app.Lib.EventBus;

namespace app.Lib.EventHandlers
{
    public class PersistanceHandler : IEventHandler<EventBufferElement>
    {
        
        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            //todo: persist to database chunks of events.
        }
    }
}