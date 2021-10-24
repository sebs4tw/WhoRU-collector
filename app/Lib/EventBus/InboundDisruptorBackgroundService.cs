using System;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Microsoft.Extensions.Hosting;
using app.Lib.EventHandlers;
using app.Lib.Configuration;

namespace app.Lib.EventBus
{

    public class InboundDisruptorBackgroundService : IHostedService, IDisposable, IInboundEventBus
    {
        public readonly uint InputRingBufferSize;
        private readonly Disruptor<EventBufferElement> inputDisruptor;
        private RingBuffer<EventBufferElement> ringBuffer;
        private bool started;

        public InboundDisruptorBackgroundService(InboundDisruptorConfiguration configuration)
        {
            InputRingBufferSize = configuration.RingBufferSize;
            inputDisruptor = new Disruptor.Dsl.Disruptor<EventBufferElement>(
                () => new EventBufferElement(),
                (int) InputRingBufferSize,
                TaskScheduler.Default, 
                ProducerType.Single, 
                new BlockingSpinWaitWaitStrategy()
            );
        }

        public void RegisterHandlers(params IEventHandler<EventBufferElement>[][] handlers)
        {
            if(started)
            {
                throw new Exception("Handlers must be registered before service start.");
            }

            if(handlers.Length > 0)
            {
                var handlerGroup = inputDisruptor.HandleEventsWith(handlers[0]);

                for(int i=1;i<handlers.Length;i++)
                {
                    handlerGroup = handlerGroup.Then(handlers[i]);
                }
            }
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