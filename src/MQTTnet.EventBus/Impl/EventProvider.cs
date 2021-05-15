using MQTTnet.EventBus.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MQTTnet.EventBus.Impl
{
    public class EventProvider : IEventProvider
    {
        private readonly IDictionary<string, EventCreater> _eventCreaters;
        private readonly IDictionary<string, EventOptions> _eventOptions;

        public EventProvider() : this(null, new HashSet<EventOptions>())
        { }

        public EventProvider(IServiceProvider serviceProvider, HashSet<EventOptions> eventOptions)
        {
            _eventCreaters = eventOptions.ToDictionary(p => p.EventName, p => EventCreater.New(serviceProvider, p));
            _eventOptions = eventOptions.ToDictionary(p => p.EventName);
        }

        public MqttApplicationMessage CreateMessage<TEven>(TEven @event, string topic)
        {
            var eventType = @event.GetType();
            var options = _eventOptions.Values.FirstOrDefault(p => p.EventType == eventType);
            if (options == null)
                throw new Exception("");

            return CreateMessage(options.EventName, @event, topic);
        }

        public MqttApplicationMessage CreateMessage(string eventName, object @event, string topic)
        {
            return _eventCreaters[eventName].CreateMqttApplicationMessage(@event, topic);
        }

        public Type GetConsumerType(string eventName)
        {
            if (_eventOptions.TryGetValue(eventName, out var options))
                return options.ConsumerType;
            return null;
        }

        public Type GetConverterType(string eventName)
            => _eventOptions[eventName].ConverterType;

        private class EventCreater
        {
            public EventCreater(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                _serializerMethod = eventOptions.ConverterType.GetMethod(nameof(IEventSerializer<object>.Serialize));
                Converter = serviceProvider.GetService(eventOptions.ConverterType);
                MessageCreater = eventOptions.MessageCreater;
            }

            private readonly MethodInfo _serializerMethod;
            public object Converter { get; }
            public Action<MqttApplicationMessageBuilder> MessageCreater { get; }

            public byte[] Serialize(object @event)
                => (byte[])_serializerMethod.Invoke(Converter, new object[] { @event });

            public MqttApplicationMessage CreateMqttApplicationMessage(object @event, string topic)
            {
                var data = Serialize(@event);

                DateTime date = DateTime.Now;
                //date.date

                var builder = new MqttApplicationMessageBuilder();
                MessageCreater.Invoke(builder);
                builder.WithTopic(topic);
                builder.WithPayload(data);

                return builder.Build();
            }

            public static EventCreater New(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                if (eventOptions.MessageCreater is null)
                    eventOptions.MessageCreater = builder => builder.WithRetainFlag();

                return new EventCreater(serviceProvider, eventOptions);
            }
        }
    }
}
