using System;

namespace MQTTnet.EventBus.Exeptions
{
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException(string eventName)
            : base($"The given event '{eventName}' was not present in the mapping dictionary, check the startup configuration")
        { }

        public EventNotFoundException(object @event)
            : this(@event.GetType().FullName)
        { }
    }
}
