using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
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
        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event, string topic)
            => eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, topic));

        public static Task<MqttClientPublishResult> PublishAsync(this IEventBus eventBus, object @event)
            => eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(@event)));

        public static Task<MqttClientPublishResult> PublishAsync<TEvent>(this IEventBus eventBus, TEvent @event, ITopicPattern<TEvent> topicInfo)
            => eventBus.PublishAsync(StaticCache.EventProvider.CreateMessage(@event, StaticCache.EventProvider.GetTopic(topicInfo)));


        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, string topic)
            => eventBus.SubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo<TEvent>(topic));
        public static Task<MqttClientSubscribeResult> SubscribeAsync<TEvent>(this IEventBus eventBus, ITopicPattern<TEvent> topicInfo)
            => eventBus.SubscribeAsync<TEvent>(StaticCache.EventProvider.GetTopic(typeof(TEvent), topicInfo));

        public static Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TEvent>(this IEventBus eventBus, string topic)
            => eventBus.UnsubscribeAsync(StaticCache.EventProvider.CreateSubscriptionInfo<TEvent>(topic));
    }
}
