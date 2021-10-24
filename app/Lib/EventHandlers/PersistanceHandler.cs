using System;
using Disruptor;
using app.Lib.EventBus;
using app.Lib.QueryRepositories;
using System.Collections.Generic;
using app.Lib.Model;

namespace app.Lib.EventHandlers
{
    public class PersistanceHandler : IEventHandler<EventBufferElement>
    {
        private LoginEventsQueryRepository eventsRepo;
        private List<StoredEvent> eventsToPersist;

        public PersistanceHandler(LoginEventsQueryRepository eventsRepo)
        {
            this.eventsRepo = eventsRepo;
            eventsToPersist = new List<StoredEvent>();
        }

        public void OnEvent(EventBufferElement data, long sequence, bool endOfBatch)
        {
            if(data.Discard)
            {
                return;
            }

            eventsToPersist.Add(new StoredEvent{
                Time = data.UnmarshalledEvent.TimeStamp,
                Level = data.UnmarshalledEvent.Level.ToString(),
                ConnectionType = data.UnmarshalledEvent.Type,
                ExtraProps = data.UnmarshalledEvent.ExtraProps.ToString()
            });

            if(endOfBatch)
            {
                eventsRepo.SaveChunk(eventsToPersist);
                eventsToPersist.Clear();
            }
        }
    }
}