using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Exceptions;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Net.Sockets;

namespace MQTTnet.EventBus
{
    public class DefaultMqttPersisterConnection : IMqttPersisterConnection
    {
        private bool _disposed;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly int _retryCount;
        private readonly ILogger<DefaultMqttPersisterConnection> _logger;
        private readonly object _syncObject;

        public DefaultMqttPersisterConnection(IMqttClientOptions mqttClientOptions, ILogger<DefaultMqttPersisterConnection> logger, int retryCount = 5)
        {
            _syncObject = new object();
            _options = mqttClientOptions;
            _logger = logger;
            _retryCount = retryCount;
            _client = new MqttFactory().CreateMqttClient();
        }

        public bool IsConnected => _client != null && _client.IsConnected;
        public IMqttClient GetClient() => _client;

        public bool TryConnect()
        {
            _logger.LogInformation("Mqtt Client is trying to connect");

            lock (_syncObject)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<MqttCommunicationException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex, "Mqtt Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );
                
                policy.Execute(() => _client.ConnectAsync(_options)).GetAwaiter().GetResult();

                if (IsConnected)
                {
                    _client.UseDisconnectedHandler(e => OnDisconnected(e));
                    _logger.LogInformation($"Mqtt Client acquired a persistent connection to '{_options.ClientId}' and is subscribed to failure events");
                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: Mqtt Client connections could not be created and opened");
                    return false;
                }
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
                _options = null;
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }
    }
}
