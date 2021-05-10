using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public class EventOptions
    {
        public EventOptions(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; set; }
        public Type EventType { get; set; }
        public Type ConsumerType { get; set; }
        public Type ConverterType { get; set; }
        public Action<MqttApplicationMessageBuilder> MessageCreater { get; set; }
    }

    internal class EventOptionsEqualityComparer : IEqualityComparer<EventOptions>
    {
        public bool Equals(EventOptions x, EventOptions y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null)
                return false;
            if (y is null)
                return false;

            return x.EventName == y.EventName;
        }

        public int GetHashCode(EventOptions obj)
            => obj.EventName.GetHashCode();
    }
}
