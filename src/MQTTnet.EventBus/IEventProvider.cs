using MQTTnet.EventBus.Exeptions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventProvider : IEnumerable<EventOptions>
    {
        bool HasTopicPattern(string eventName);
        bool TrySetTopicInfo(string eventName, object @event, string topic);
        string GetTopic(string eventName);
        string GetTopic(string eventName, object topicInfo);
        string GetTopicEntity(string eventName, string topic, string name);
        bool TryGetEventName(Type eventType, out string eventName);
        bool TryGetEventOptions(string eventName, out EventOptions options);
        MqttApplicationMessage CreateMessage(string eventName, object @event, string topic);
    }

    public static class IEventProviderExtensions
    {
        public static string GetEventName<TEvenet>(this IEventProvider eventProvider) =>
            GetEventName(eventProvider, typeof(TEvenet));

        public static string GetEventName(this IEventProvider eventProvider, Type eventType)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventName;
            throw new EventNotFoundException(eventType);
        }

        public static EventOptions GetEventOptions<TEvent>(this IEventProvider eventProvider) =>
            GetEventOptions(eventProvider, typeof(TEvent));

        public static EventOptions GetEventOptions(this IEventProvider eventProvider, Type eventType)
        {
            if (TryGetEventOptions(eventProvider, eventType, out var options))
                return options;

            throw new EventNotFoundException(eventType);
        }

        public static bool TryGetEventOptions<TEvent>(this IEventProvider eventProvider, out EventOptions options) =>
            TryGetEventOptions(eventProvider, typeof(TEvent), out options);

        public static bool TryGetEventOptions(this IEventProvider eventProvider, Type eventType, out EventOptions options)
        {
            if (eventProvider.TryGetEventName(eventType, out var eventname))
                return eventProvider.TryGetEventOptions(eventname, out options);

            options = null;
            return false;
        }

        public static Type GetConsumerType(this IEventProvider eventProvider, string eventName)
        {
            if (eventProvider.TryGetEventOptions(eventName, out var options))
                return options.ConsumerType;
            return null;
        }

        public static Type GetConverterType(this IEventProvider eventProvider, string eventName)
        {
            if (eventProvider.TryGetEventOptions(eventName, out var options))
                return options.ConverterType;
            return null;
        }

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

        public static string GetTopic(this IEventProvider eventProvider, Type eventType)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.GetTopic(eventName);
            return null;
        }

        public static string GetTopic(this IEventProvider eventProvider, object @event) => 
            GetTopic(eventProvider, @event.GetType());

        public static string GetTopic(this IEventProvider eventProvider, Type eventType, object toipcInfo)
        {
            if (eventProvider.TryGetEventName(eventType, out string eventName))
                return eventProvider.GetTopic(eventName, toipcInfo);
            return null;
        }

        public static string GetTopic<TEvent>(this IEventProvider eventProvider, ITopicPattern<TEvent> toipicInfo) => 
            GetTopic(eventProvider, typeof(TEvent), toipicInfo);

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

        public static Type GetConsumerType<TEvent>(this IEventProvider eventProvider) => 
            eventProvider.GetConsumerType(typeof(TEvent));

        public static SubscriptionInfo CreateSubscriptionInfo(this IEventProvider eventProvider, string eventName, Type eventType, string topic) =>
            new SubscriptionInfo
            {
                Topic = topic,
                EventName = eventName,
                EventType = eventType,
                ConsumerType = eventProvider.GetConsumerType(eventType)
            };

        public static SubscriptionInfo CreateSubscriptionInfo(this IEventProvider eventProvider, Type eventType, string topic) =>
            eventProvider.CreateSubscriptionInfo(eventType.Name, eventType, topic);

        public static SubscriptionInfo CreateSubscriptionInfo<TEvent>(this IEventProvider eventProvider, string topic) => 
            CreateSubscriptionInfo(eventProvider, typeof(TEvent), topic);

        public static async IAsyncEnumerable<TResult> ExecuteForAllEventsAsync<TResult>(this IEventProvider eventProvider, Func<EventOptions, Task<TResult>> executer, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var info in eventProvider)
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                yield return await executer.Invoke(info);
            }
        }
    }
}
