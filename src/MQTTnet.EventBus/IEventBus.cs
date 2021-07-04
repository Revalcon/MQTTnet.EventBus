using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBus
    {
        Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken = default);
        Task<MqttClientSubscribeResult> SubscribeAsync(SubscriptionInfo subscriptionInfo, CancellationToken cancellationToken = default);
        Task<MqttClientUnsubscribeResult> UnsubscribeAsync(SubscriptionInfo subscriptionInfo, CancellationToken cancellationToken = default);
        Task<MqttClientSubscribeResult[]> ReSubscribeAllAsync(CancellationToken cancellationToken = default);
    }

    public static class EventBusExtensions
    {
        //PublishAsync extensions
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, string eventName, object @event, string topic, CancellationToken cancellationToken = default) =>
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(eventName, @event, topic), cancellationToken);
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, string topic, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, topic), cancellationToken);
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(@event)), cancellationToken);
        public static Task<MqttClientPublishResult> PublishAsync<TEvent>(this IEventBus eventBus, TEvent @event, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(topicInfo)), cancellationToken);

        //SubscribeAsync extensions
        public static Task<MqttClientSubscribeResult> SubscribeAsync(this IEventBus eventBus, string eventName, Type eventType, string topic, CancellationToken cancellationToken = default) =>
            eventBus.SubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventName, eventType, topic), cancellationToken);
        public static Task<MqttClientSubscribeResult> SubscribeAsync(this IEventBus eventBus, Type eventType, string topic, CancellationToken cancellationToken = default) => 
            SubscribeAsync(eventBus, eventType.Name, eventType, topic, cancellationToken);
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, string topic, CancellationToken cancellationToken = default) => 
            SubscribeAsync(eventBus, typeof(TEvent), topic, cancellationToken);
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            SubscribeAsync<TEvent>(eventBus, StaticCache.EventProvider.GetTopic(topicInfo), cancellationToken);

        //UnsubscribeAsync extensions
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync(this IEventBus eventBus, string eventName, Type eventType, string topic, CancellationToken cancellationToken = default) =>
            eventBus.UnsubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventName, eventType, topic), cancellationToken);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync(this IEventBus eventBus, Type eventType, string topic, CancellationToken cancellationToken = default) => 
            UnsubscribeAsync(eventBus, eventType.Name, eventType, topic, cancellationToken);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, string topic, CancellationToken cancellationToken = default) => 
            UnsubscribeAsync(eventBus, typeof(TEvent), topic, cancellationToken);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            UnsubscribeAsync<TEvent>(eventBus, StaticCache.EventProvider.GetTopic(topicInfo), cancellationToken);

        public static Task<List<MqttClientSubscribeResult>> SubscribeAllAsync(this IEventBus eventBus, CancellationToken cancellationToken = default) =>
            StaticCache.EventProvider
                .ExecuteForAllEventsAsync(info => eventBus.SubscribeAsync(info.EventName, info.EventType, info.Topic.RootTopic), cancellationToken)
                .ToListAsync();
        public static Task<List<MqttClientUnsubscribeResult>> UnsubscribeAllAsync(this IEventBus eventBus, CancellationToken cancellationToken = default) =>
            StaticCache.EventProvider
                .ExecuteForAllEventsAsync(info => eventBus.UnsubscribeAsync(info.EventName, info.EventType, info.Topic.RootTopic), cancellationToken)
                .ToListAsync();
    }
}
