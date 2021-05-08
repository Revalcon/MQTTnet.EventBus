using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTnet.EventBus
{
    public interface IEventProvider
    {
        Type GetConsumerType(string eventName);
        object GetConverter(string eventName);
        MqttApplicationMessage CreateMessage(string eventName, string topic);
    }

    public class EventProvider : IEventProvider
    {
        private readonly IDictionary<string, EventCreater> _eventCreaters;
        private readonly IDictionary<string, EventOptions> _eventOptions;

        public EventProvider() : this(new HashSet<EventOptions>())
        { }

        public EventProvider(HashSet<EventOptions> eventOptions)
        {
            _eventCreaters = eventOptions.ToDictionary(p => p.EventName, p => EventCreater.New(p));
            _eventOptions = eventOptions.ToDictionary(p => p.EventName);
        }

        public MqttApplicationMessage CreateMessage(string eventName, string topic)
        {
            var eventCreater = _eventCreaters[eventName];
            //var data = (byte[])eventCreater.Converter.Serialize(@event);
            byte[] data = null;

            var builder = new MqttApplicationMessageBuilder();
            eventCreater.MessageCreater.Invoke(builder);
            builder.WithTopic(topic);
            builder.WithPayload(data);

            return builder.Build();
        }

        public object GetConverter(string eventName)
            => _eventCreaters[eventName].Converter;

        public Type GetConsumerType(string eventName)
        {
            if (_eventOptions.TryGetValue(eventName, out var options))
                return options.ConsumerType;
            return null;
        }

        private class EventCreater
        {
            public EventCreater(dynamic converter, Action<MqttApplicationMessageBuilder> messageCreater)
            {
                Converter = converter;
                MessageCreater = messageCreater;
            }

            public dynamic Converter { get; }
            public Action<MqttApplicationMessageBuilder> MessageCreater { get; }

            public static EventCreater New(EventOptions eventOptions)
            {
                if(eventOptions.MessageCreater is null)
                    eventOptions.MessageCreater = builder => builder.WithRetainFlag();

                return new EventCreater(Activator.CreateInstance(eventOptions.ConverterType), eventOptions.MessageCreater);
            }
        }
    }

    public static class IEventProviderExtensions
    {
        public static Type GetConsumerType(this IEventProvider eventProvider, object @event)
            => eventProvider.GetConsumerType(@event.GetType().Name);
        public static object GetConverter(this IEventProvider eventProvider, object @event)
            => eventProvider.GetConverter(@event.GetType().Name);
        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event, string topic)
            => eventProvider.CreateMessage(@event.GetType().Name, topic);

        public static MqttApplicationMessage CreateMessage<TEvent>(this IEventProvider eventProvider, string topic, TEvent @event)
            => eventProvider.CreateMessage(topic, @event);

        public static Type GetConsumerType<TEvent>(this IEventProvider eventProvider)
            => eventProvider.GetConsumerType(typeof(TEvent));

        public static SubscriptionInfo CreateSubscriptionInfo(this IEventProvider eventProvider, Type eventType, string topic)
            => new SubscriptionInfo
            {
                Topic = topic,
                EventType = eventType,
                ConsumerType = eventProvider.GetConsumerType(eventType)
            };

        public static SubscriptionInfo CreateSubscriptionInfo<TEvent>(this IEventProvider eventProvider, string topic)
            => CreateSubscriptionInfo(eventProvider, typeof(TEvent), topic);
    }
}
