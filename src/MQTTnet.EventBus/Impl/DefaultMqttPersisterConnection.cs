using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.EventBus.Logger;
using MQTTnet.Exceptions;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Net.Sockets;

namespace MQTTnet.EventBus.Impl
{
    public class DefaultMqttPersisterConnection : IMqttPersisterConnection
    {
        private bool _disposed;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly int _retryCount;
        private readonly IEventBusLogger<DefaultMqttPersisterConnection> _logger;
        private readonly object _syncObject;

        public DefaultMqttPersisterConnection(IMqttClientOptions mqttClientOptions, IEventBusLogger<DefaultMqttPersisterConnection> logger, BusOptions busOptions)
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
                var policy = Policy.Handle<SocketException>()
                    .Or<MqttProtocolViolationException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex, $"Mqtt Client could not connect after {time.TotalSeconds:n1}s ({ex.Message})");
                    }
                );

                policy.Execute(() => _client.ConnectAsync(_options)).GetAwaiter().GetResult();
                return IsConnected;
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
                _logger.LogError(ex, $"MqttClient cann't Dispose");
            }
        }
    }
}
