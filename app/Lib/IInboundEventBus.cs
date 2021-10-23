using System;

namespace app.Lib
{
    public interface IInboundEventBus
    {
        void Publish(string serializedMsg);
    }
}