using System;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Microsoft.Extensions.Hosting;
using app.Lib.EventHandlers;

namespace app.Lib.EventBus
{

    public class InboundDisruptorBackgroundService : IHostedService, IDisposable, IInboundEventBus
    {
        //todo: extract configuration
        public const int InputRingBufferSize = 8192;
        private readonly Disruptor<EventBufferElement> inputDisruptor;
        private RingBuffer<EventBufferElement> ringBuffer;
        private bool started;

        public InboundDisruptorBackgroundService(params IEventHandler<EventBufferElement>[] handlers)
        {
            inputDisruptor = new Disruptor.Dsl.Disruptor<EventBufferElement>(
                () => new EventBufferElement(),
                InputRingBufferSize,
                TaskScheduler.Default, 
                ProducerType.Single, 
                new BlockingSpinWaitWaitStrategy()
            );

            inputDisruptor.HandleEventsWith(handlers);
        }

        public void Dispose()
        {
            if(started)
            {
                throw new Exception("Service must be stopped before it can be disposed of to ensure data integrity.");
            }
        }

        public void Publish(string serializedMsg)
        {
            if(!started)
            {
                throw new Exception("Service is not running.");
            }

            var nextIndex = ringBuffer.Next();
            try
            {
                var element = ringBuffer[nextIndex];
                element.Reset(serializedMsg);
            }
            finally
            {
                ringBuffer.Publish(nextIndex);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(!started)
            {
                started = true;
                ringBuffer = inputDisruptor.Start();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if(started)
            {
                //will wait until everything in the buffer is fully processed
                inputDisruptor.Shutdown();
                ringBuffer = null;
                started = false;
            }

            return Task.CompletedTask;
        }
    }
}