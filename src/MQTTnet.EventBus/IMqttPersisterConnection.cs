using MQTTnet.Client;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.EventBus.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IMqttPersisterConnection : IDisposable
    {
        bool IsConnected { get; }
        Task<bool> TryConnectAsync(bool afterDisconnection = false, CancellationToken cancellationToken = default);
        IMqttClient GetClient();
        event Func<IMqttPersisterConnection, MqttClientConnectionEventArgs, Task> ClientConnectionChanged;
    }

    public static class IMqttPersisterConnectionExtensions
    {
        public static async Task<bool> TryRegisterMessageHandlerAsync(this IMqttPersisterConnection persisterConnection, Func<MqttApplicationMessageReceivedEventArgs, Task> handler, CancellationToken cancellationToken = default)
        {
            if (await persisterConnection.TryConnectAsync(afterDisconnection: false, cancellationToken))
            {
                persisterConnection.GetClient().UseApplicationMessageReceivedHandler(handler);
                return true;
            }
            return false;
        }

        public static async Task<MqttClientSubscribeResult> SubscribeAsync(this IMqttPersisterConnection persisterConnection, string topic, CancellationToken cancellationToken = default)
        {
            if (await persisterConnection.TryConnectAsync(afterDisconnection: false, cancellationToken))
                return await persisterConnection.GetClient().SubscribeAsync(topic);
            return null;
        }

        public static async Task<MqttClientUnsubscribeResult> RemoveSubscriptionAsync(this IMqttPersisterConnection persisterConnection, string topic, CancellationToken cancellationToken = default)
        {
            if (await persisterConnection.TryConnectAsync(afterDisconnection: false, cancellationToken))
                return await persisterConnection.GetClient().UnsubscribeAsync(topic);
            return null;
        }
    }
}