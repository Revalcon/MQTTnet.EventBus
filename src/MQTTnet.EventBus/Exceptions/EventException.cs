using System;

namespace MQTTnet.EventBus.Exceptions
{
    public class EventException : Exception
    {
        public EventException(string eventName, Type eventType, string message)
            : base(message)
        {
            EventName = eventName;
            EventType = eventType;
        }

        public string EventName { get; }
        public Type EventType { get; }
    }
}
