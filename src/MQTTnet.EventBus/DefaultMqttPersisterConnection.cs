using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.Exceptions;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IEventBusClientProvider : IDisposable
    {
        IMqttPersisterConnection GetOrCreateMqttConnection(string topicPath);
        bool RegisterMessageHandler(string topic, Action<MqttApplicationMessageReceivedEventArgs> handler, out IMqttPersisterConnection persisterConnection);
        Task<MqttClientUnsubscribeResult> RemoveSubscriptionAsync(string topic);
    }

    public class EventBusClientProvider : IEventBusClientProvider
    {
        private bool _disposed;
        private readonly IMqttClientOptions _options;
        private readonly BusOptions _busOptions;
        private readonly ILogger<EventBusClientProvider> _logger;
        private readonly object _syncObject;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IDictionary<string, IMqttPersisterConnection> _persisterConnections;

        public EventBusClientProvider(IMqttClientOptions options, BusOptions busOptions, ILoggerFactory loggerFactory)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<EventBusClientProvider>();
            _busOptions = busOptions;
            _loggerFactory = loggerFactory;
            _persisterConnections = new Dictionary<string, IMqttPersisterConnection>();
            _syncObject = new object();
        }

        public IMqttPersisterConnection GetOrCreateMqttConnection(string topic)
        {
            lock (_syncObject)
            {
                if (TryGetMqttConnection(topic, out var connection))
                    return connection;

                connection = new DefaultMqttPersisterConnection(_options, _loggerFactory.CreateLogger<DefaultMqttPersisterConnection>(), _busOptions);
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

    public class DefaultMqttPersisterConnection : IMqttPersisterConnection
    {
        private bool _disposed;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly int _retryCount;
        private readonly ILogger _logger;
        private readonly object _syncObject;

        public DefaultMqttPersisterConnection(IMqttClientOptions mqttClientOptions, ILogger logger, BusOptions busOptions)
        {
            _syncObject = new object();
            _options = mqttClientOptions;
            _logger = logger;
            _retryCount = busOptions?.RetryCount ?? 5;
            _client = CreareMqttClient();
        }

        public bool IsConnected => _client != null && _client.IsConnected;
        public IMqttClient GetClient() => _client;

        private IMqttClient CreareMqttClient()
        {
            var client = new MqttFactory().CreateMqttClient();
            client.UseDisconnectedHandler(e => OnDisconnected(e));
            _logger.LogInformation($"Mqtt Client acquired a persistent connection to '{_options.ClientId}' and is subscribed to failure events");
            return client;
        }

        public bool TryConnect()
        {
            _logger.LogInformation("Mqtt Client is trying to connect");

            lock (_syncObject)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<MqttProtocolViolationException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex, $"Mqtt Client could not connect after {time.TotalSeconds:n1}s ({ex.Message})");
                    }
                );

                policy.Execute(() => _client.ConnectAsync(_options)).GetAwaiter().GetResult();
                return IsConnected;

                //if (IsConnected)
                //{
                //    _client.UseDisconnectedHandler(e => OnDisconnected(e));
                //    _logger.LogInformation($"Mqtt Client acquired a persistent connection to '{_options.ClientId}' and is subscribed to failure events");
                //    return true;
                //}
                //else
                //{
                //    _logger.LogCritical("FATAL ERROR: Mqtt Client connections could not be created and opened");
                //    return false;
                //}
            }
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            if (_disposed)
                return;

            _logger.LogWarning("A MqttServer connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _client?.Dispose();
                _client = null;
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }
    }
}
