using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBus
    {
        Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message);
        Task<MqttClientSubscribeResult> SubscribeAsync<TH>(string topic)
            where TH : IIntegrationEventHandler;
        Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TH>(string topic)
            where TH : IIntegrationEventHandler;
    }
}
