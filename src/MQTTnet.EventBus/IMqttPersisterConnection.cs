using MQTTnet.Client;
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
        public static async Task<IMqttPersisterConnection> RegisterMessageHandlerAsync(this IMqttPersisterConnection persisterConnection, Func<MqttApplicationMessageReceivedEventArgs, Task> handler)
        {
            if (await persisterConnection.TryConnectAsync())
                persisterConnection.GetClient().UseApplicationMessageReceivedHandler(handler);

            return persisterConnection;
        }

        public static async Task<MqttClientUnsubscribeResult> RemoveSubscriptionAsync(this IMqttPersisterConnection persisterConnection, string topic)
        {
            var connection = persisterConnection;
            if (connection.IsConnected || await connection.TryConnectAsync())
                return await connection.GetClient().UnsubscribeAsync(topic);
            return null;
        }
    }
}