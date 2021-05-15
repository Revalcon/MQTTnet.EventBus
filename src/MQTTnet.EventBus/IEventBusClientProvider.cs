using MQTTnet.Client.Unsubscribing;
using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBusClientProvider : IDisposable
    {
        IMqttPersisterConnection GetOrCreateMqttConnection(string topicPath);
        bool RegisterMessageHandler(string topic, Action<MqttApplicationMessageReceivedEventArgs> handler, out IMqttPersisterConnection persisterConnection);
        Task<MqttClientUnsubscribeResult> RemoveSubscriptionAsync(string topic);
    }
}
