using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBus
    {
        Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken = default);
        Task<MqttClientSubscribeResult> SubscribeAsync(SubscriptionInfo subscriptionInfo, CancellationToken cancellationToken = default);
        Task<MqttClientUnsubscribeResult> UnsubscribeAsync(SubscriptionInfo subscriptionInfo, CancellationToken cancellationToken = default);
        Task<MqttClientSubscribeResult[]> ReSubscribeAllTopicsAsync(CancellationToken cancellationToken = default);
    }

    public static class EventBusExtensions
    {
        //PublishAsync extensions
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, string topic, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, topic), cancellationToken);
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(@event)), cancellationToken);
        public static Task<MqttClientPublishResult> PublishAsync<TEvent>(this IEventBus eventBus, TEvent @event, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(topicInfo)), cancellationToken);

        //SubscribeAsync extensions
        public static Task<MqttClientSubscribeResult> SubscribeAsync(this IEventBus eventBus, Type eventType, string topic, CancellationToken cancellationToken = default) => 
            eventBus.SubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventType, topic), cancellationToken);
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, string topic, CancellationToken cancellationToken = default) => 
            eventBus.SubscribeAsync(typeof(TEvent), topic, cancellationToken);
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            eventBus.SubscribeAsync<TEvent>(StaticCache.EventProvider.GetTopic(topicInfo), cancellationToken);

        //UnsubscribeAsync extensions
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync(this IEventBus eventBus, Type eventType, string topic, CancellationToken cancellationToken = default) => 
            eventBus.UnsubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventType, topic), cancellationToken);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, string topic, CancellationToken cancellationToken = default) => 
            eventBus.UnsubscribeAsync(typeof(TEvent), topic, cancellationToken);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo, CancellationToken cancellationToken = default) => 
            eventBus.UnsubscribeAsync<TEvent>(StaticCache.EventProvider.GetTopic(topicInfo), cancellationToken);
    }
}
