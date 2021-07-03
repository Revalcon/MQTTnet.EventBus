using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBus
    {
        Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message);
        Task<MqttClientSubscribeResult> SubscribeAsync(SubscriptionInfo subscriptionInfo);
        Task<MqttClientUnsubscribeResult> UnsubscribeAsync(SubscriptionInfo subscriptionInfo);
        Task<MqttClientSubscribeResult[]> ReSubscribeAllTopicsAsync();
    }

    public static class EventBusExtensions
    {
        //PublishAsync extensions
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, string topic) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, topic));
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(@event)));
        public static Task<MqttClientPublishResult> PublishAsync<TEvent>(this IEventBus eventBus, TEvent @event, ITopicPattern<TEvent> topicInfo) => 
            eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(topicInfo)));

        //SubscribeAsync extensions
        public static Task<MqttClientSubscribeResult> SubscribeAsync(this IEventBus eventBus, Type eventType, string topic) => 
            eventBus.SubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventType, topic));
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, string topic) => 
            eventBus.SubscribeAsync(typeof(TEvent), topic);
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo) => 
            eventBus.SubscribeAsync<TEvent>(StaticCache.EventProvider.GetTopic(topicInfo));

        //UnsubscribeAsync extensions
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync(this IEventBus eventBus, Type eventType, string topic) => 
            eventBus.UnsubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo(eventType, topic));
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, string topic) => 
            eventBus.UnsubscribeAsync(typeof(TEvent), topic);
        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo) => 
            eventBus.UnsubscribeAsync<TEvent>(StaticCache.EventProvider.GetTopic(topicInfo));
    }
}
