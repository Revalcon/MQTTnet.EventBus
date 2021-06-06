using System;

namespace MQTTnet.EventBus
{
    public static class IEventProviderExtensions
    {
        public static string GetTopicEntity(this IEventProvider eventProvider, Type eventType, string topic, string name)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.GetTopicEntity(eventName, topic, name);
            return null;
        }

        public static bool HasTopicPattern(this IEventProvider eventProvider, Type eventType)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.HasTopicPattern(eventName);
            return false;
        }

        public static bool TrySetTopicInfo<TModel>(this IEventProvider eventProvider, TModel model, Type eventType, string topic)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.TrySetTopicInfo(eventName, model, topic);
            return false;
        }

        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, string eventName, object @event, object topicInfo)
        {
            string topic = eventProvider.GetTopic(eventName, topicInfo);
            return eventProvider.CreateMessage(eventName, @event, topic);
        }

        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event, object topicInfo)
        {
            if (eventProvider.TryGetEventName(@event.GetType(), out string eventName))
                return eventProvider.CreateMessage(eventName, @event, topicInfo);
            return null;
        }

        public static bool SetTopicInfo(this IEventProvider eventProvider, object @event, string topic)
        {
            if (eventProvider.TryGetEventName(@event.GetType(), out string eventName))
                return eventProvider.TrySetTopicInfo(eventName, @event, topic);
            return false;
        }

        public static string GetTopic(this IEventProvider eventProvider, object @event, object topicInfo)
            => GetTopic(eventProvider, @event.GetType(), topicInfo);

        public static string GetTopic(this IEventProvider eventProvider, Type eventType, object toipcInfo)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.GetTopic(eventName, toipcInfo);
            return null;
        }

        public static Type GetConsumerType(this IEventProvider eventProvider, Type eventType)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.GetConsumerType(eventName);
            return null;
        }

        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event, string topic)
        {
            if (eventProvider.TryGetEventName(@event.GetType(), out string eventName))
                return eventProvider.CreateMessage(eventName, @event, topic);
            return null;
        }

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
