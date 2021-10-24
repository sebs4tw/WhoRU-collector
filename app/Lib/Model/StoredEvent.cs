using System;

namespace app.Lib.Model
{
    public class StoredEvent
    {
        public DateTime Time;
        public string ConnectionType;
        public string Level;
        public string ExtraProps { get; set; }
    }

}