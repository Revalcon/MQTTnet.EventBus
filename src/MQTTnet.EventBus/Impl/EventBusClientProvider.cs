using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.EventBus.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Impl
{
    public class EventBusClientProvider : IEventBusClientProvider
    {
        private bool _disposed;
        private readonly IMqttClientOptions _options;
        private readonly BusOptions _busOptions;
        private readonly IEventBusLogger<EventBusClientProvider> _logger;
        private readonly object _syncObject;

        private readonly IDictionary<string, IMqttPersisterConnection> _persisterConnections;

        public EventBusClientProvider(IMqttClientOptions options, BusOptions busOptions, IEventBusLogger<EventBusClientProvider> logger)
        {
            _options = options;
            _logger = logger;
            _busOptions = busOptions;
            _persisterConnections = new Dictionary<string, IMqttPersisterConnection>();
            _syncObject = new object();
        }

        public IMqttPersisterConnection GetOrCreateMqttConnection(string topic)
        {
            lock (_syncObject)
            {
                if (TryGetMqttConnection(topic, out var connection))
                    return connection;

                connection = new DefaultMqttPersisterConnection(_options, _logger.CreateLogger<DefaultMqttPersisterConnection>(), _busOptions);
                _persisterConnections.Add(topic, connection);
                return connection;
            }
        }

        public bool RegisterMessageHandler(string topic, Action<MqttApplicationMessageReceivedEventArgs> handler, out IMqttPersisterConnection persisterConnection)
        {
            persisterConnection = GetOrCreateMqttConnection(topic);
            if (persisterConnection.TryConnect())
                persisterConnection.GetClient().UseApplicationMessageReceivedHandler(handler);

            return persisterConnection.IsConnected;
        }

        private bool TryGetMqttConnection(string topic, out IMqttPersisterConnection connection)
            => _persisterConnections.TryGetValue(topic, out connection);

        public async Task<MqttClientUnsubscribeResult> RemoveSubscriptionAsync(string topic)
        {
            if (TryGetMqttConnection(topic, out var connection))
            {
                if (!connection.IsConnected)
                {
                    if (connection.TryConnect())
                    {
                        return await connection.GetClient().UnsubscribeAsync(topic);
                    }
                }

                _persisterConnections.Remove(topic);
            }

            return null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_syncObject)
            {
                foreach (var conn in _persisterConnections)
                {
                    conn.Value.Dispose();
                }
            }
            _persisterConnections?.Clear();

            _disposed = true;
        }
    }
}
