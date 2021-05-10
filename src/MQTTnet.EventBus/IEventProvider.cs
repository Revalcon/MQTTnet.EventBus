using MQTTnet.EventBus.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MQTTnet.EventBus
{
    public interface IEventProvider
    {
        Type GetConverterType(string eventName);
        Type GetConsumerType(string eventName);
        MqttApplicationMessage CreateMessage(string eventName, object @event, string topic);
    }

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
            public EventCreater(IServiceProvider serviceProvider, Type consumerType, Action<MqttApplicationMessageBuilder> messageCreater)
            {
                _serializerMethod = consumerType.GetMethod(nameof(IEventSerializer<object>.Serialize));
                Converter = serviceProvider.GetService(consumerType);
                MessageCreater = messageCreater;
            }

            private readonly MethodInfo _serializerMethod;
            public object Converter { get; }
            public Action<MqttApplicationMessageBuilder> MessageCreater { get; }

            public byte[] Serialize(object @event)
                => (byte[])_serializerMethod.Invoke(Converter, new object[] { @event });

            public MqttApplicationMessage CreateMqttApplicationMessage(object @event, string topic)
            {
                var data = Serialize(@event);

                var builder = new MqttApplicationMessageBuilder();
                MessageCreater.Invoke(builder);
                builder.WithTopic(topic);
                builder.WithPayload(data);

                return builder.Build();
            }

            public static EventCreater New(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                if(eventOptions.MessageCreater is null)
                    eventOptions.MessageCreater = builder => builder.WithRetainFlag();

                return new EventCreater(serviceProvider, eventOptions.ConsumerType, eventOptions.MessageCreater);
            }
        }
    }

    public static class IEventProviderExtensions
    {
        public static Type GetConsumerType(this IEventProvider eventProvider, Type eventType)
            => eventProvider.GetConsumerType(eventType.Name);
        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event, string topic)
            => eventProvider.CreateMessage(@event.GetType().Name, @event, topic);

        public static Type GetConsumerType<TEvent>(this IEventProvider eventProvider)
            => eventProvider.GetConsumerType(typeof(TEvent));

        public static SubscriptionInfo CreateSubscriptionInfo(this IEventProvider eventProvider, Type eventType, string topic)
            => new SubscriptionInfo
            {
                Topic = topic,
                EventName = eventType.Name,
                EventType = eventType,
                ConsumerType = eventProvider.GetConsumerType(eventType)
            };

        public static SubscriptionInfo CreateSubscriptionInfo<TEvent>(this IEventProvider eventProvider, string topic)
            => CreateSubscriptionInfo(eventProvider, typeof(TEvent), topic);
    }
}
