using System;

namespace MQTTnet.EventBus
{
    public interface IEventProvider
    {
        bool SetTopicInfo(string eventName, object @event, string topic);
        string GetTopic(string eventName, object @event);
        Type GetConverterType(string eventName);
        Type GetConsumerType(string eventName);
        MqttApplicationMessage CreateMessage(string eventName, object @event, string topic);
    }

    public static class IEventProviderExtensions
    {


        public static MqttApplicationMessage CreateMessage<TEvent>(this IEventProvider eventProvider, string eventName, TEvent @event)
        {
            string topic = eventProvider.GetTopic(@event);
            return eventProvider.CreateMessage(eventName, @event, topic);
        }

        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event)
            => CreateMessage(eventProvider, @event.GetType().Name, @event);

        public static bool SetTopicInfo(this IEventProvider eventProvider, object @event, string topic)
            => eventProvider.SetTopicInfo(@event.GetType().Name, @event, topic);
        public static string GetTopic(this IEventProvider eventProvider, object @event)
            => eventProvider.GetTopic(@event.GetType().Name, @event);
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
