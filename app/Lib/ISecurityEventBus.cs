using System;
using System.Collections.Generic;
using app.Lib.Model;

namespace app.Lib
{
    public interface ISecurityEventBus
    {
        void Publish(IEnumerable<SecurityEvent> securityEvents);
    }
}