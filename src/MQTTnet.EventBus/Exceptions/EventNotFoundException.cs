using System;

namespace MQTTnet.EventBus.Exceptions
{
    public class EventNotFoundException : EventException
    {
        public EventNotFoundException(string eventName, Type eventType)
            : base(eventName, eventType, $"The given event '{eventName}' was not present in the mapping dictionary, check the startup configuration")
        { }
    }
}
