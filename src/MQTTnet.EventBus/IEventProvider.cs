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
        MqttApplicationMessage CreateMessage<TEven>(TEven @event, string topic);
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
