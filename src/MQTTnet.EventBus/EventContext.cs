using MQTTnet.EventBus.Serializers;
using System;

namespace MQTTnet.EventBus
{
    public class EventContext
    {
        private readonly MqttApplicationMessageReceivedEventArgs message;

        public EventContext(MqttApplicationMessageReceivedEventArgs message)
        {
            this.message = message;
        }

        public string ClientId => message.ClientId;
        public bool ProcessingFailed => message.ProcessingFailed;
        public MqttApplicationMessage Message => message.ApplicationMessage;
    }

    public class EventContext<T> : EventContext
    {
        private readonly Lazy<T> _lazyConverter;

        public EventContext(IEventDeserializer<T> deserializer, MqttApplicationMessageReceivedEventArgs message)
            : this(new Lazy<T>(() => deserializer.Deserialize(message?.ApplicationMessage?.Payload)), message)
        { }

        public EventContext(Lazy<T> lazyConverter, MqttApplicationMessageReceivedEventArgs message)
            : base(message)
        {
            _lazyConverter = lazyConverter;
        }

        public T EventArg => _lazyConverter.Value;
    }
}
