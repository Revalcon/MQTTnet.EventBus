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

        public static MqttApplicationMessage CreateMessage<TEvent>(this IEventProvider eventProvider, string eventName, TEvent @event)
        {
            string topic = eventProvider.GetTopic(@event);
            return eventProvider.CreateMessage(eventName, @event, topic);
        }

        public static MqttApplicationMessage CreateMessage(this IEventProvider eventProvider, object @event)
            => CreateMessage(eventProvider, @event.GetType().Name, @event);

        public static bool SetTopicInfo(this IEventProvider eventProvider, object @event, string topic)
            => eventProvider.TrySetTopicInfo(@event.GetType().Name, @event, topic);

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
