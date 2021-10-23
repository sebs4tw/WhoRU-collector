using System;
using Disruptor;
using app.Lib.EventBus;

namespace app.Lib.EventHandlers
{
    public class UnmarshallingHandler : IEventHandler<EventBufferElement>
    {
        
        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            Console.WriteLine(data.RawMessage);
            //todo: deserialize messages into ring buffer
        }
    }
}