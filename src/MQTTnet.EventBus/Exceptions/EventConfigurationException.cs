using System;

namespace MQTTnet.EventBus.Exceptions
{
    public class EventConfigurationException : EventException
    {
        public EventConfigurationException(string eventName, Type eventType, string message)
            : base(eventName, eventType, message)
        { }
    }
}
