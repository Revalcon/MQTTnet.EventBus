using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.EventBus.Logger;
using MQTTnet.Exceptions;
using MQTTnet.Internal;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Impl
{
    public class DefaultMqttPersisterConnection : IMqttPersisterConnection
    {
        private bool _disposed;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly int _retryCount;
        private readonly IEventBusLogger<DefaultMqttPersisterConnection> _logger;
        private readonly IDictionary<string, MqttClientConnectionEventArgs> _disconnectionCache;
        private readonly AsyncLock _asyncLock;

        public DefaultMqttPersisterConnection(IMqttClientOptions mqttClientOptions, IEventBusLogger<DefaultMqttPersisterConnection> logger, BusOptions busOptions)
        {
            _asyncLock = new AsyncLock();
            _options = mqttClientOptions;
            _logger = logger;
            _retryCount = busOptions?.RetryCount ?? 5;
            _client = CreareMqttClient();
            _disconnectionCache = new Dictionary<string, MqttClientConnectionEventArgs>();
        }

        public event Func<IMqttPersisterConnection, MqttClientConnectionEventArgs, Task> ClientConnectionChanged;
        public bool IsConnected => _client.IsConnected;
        public IMqttClient GetClient() => _client;

        private IMqttClient CreareMqttClient()
        {
            var client = new MqttFactory().CreateMqttClient();
            client.UseDisconnectedHandler(OnDisconnectedAsync);
            client.UseConnectedHandler(OnConnectedAsync);
            _logger.LogInformation($"Mqtt Client acquired a persistent connection to '{_options.ClientId}'");
            return client;
        }

        public async Task<bool> TryConnectAsync(bool afterDisconnection = false, CancellationToken cancellationToken = default)
        {
            if (IsConnected)
                return true;

            using (await _asyncLock.WaitAsync(cancellationToken))
            {
                _logger.LogInformation("Mqtt Client is trying to connect");

                try
                {
                    var policy = Policy.Handle<SocketException>()
                        .Or<MqttProtocolViolationException>()
                        .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                        {
                            _logger.LogWarning(ex, $"Mqtt Client could not connect after {time.TotalSeconds:n1}s ({ex.Message})");
                        });

                    await policy.ExecuteAsync(token => afterDisconnection ?
                        _client.ReconnectAsync() :
                        _client.ConnectAsync(_options, token), cancellationToken);

                    _logger.LogInformation("Mqtt Client was connected");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                }

                return IsConnected;
            }
        }

        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            if (_disconnectionCache.TryGetValue(_options.ClientId, out var args))
            {
                if(!args.IsReConnected)
                {
                    _logger.LogInformation($"A MqttServer '{_options.ClientId}' trying to connect...");
                    args.MarkAsConnected();
                    return InvokeClientConnectionChangedMethod(args);
                }
            }
            else
                _disconnectionCache.Add(_options.ClientId, MqttClientConnectionEventArgs.Connected(_options.ClientId));

            return Task.CompletedTask;
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            if (_disposed)
                return;

            _logger.LogWarning($"A MqttServer '{_options.ClientId}' connection is shutdown, Reason: {e.Reason}. Trying to re-connect...");
            if (_disconnectionCache.TryGetValue(_options.ClientId, out var args))
            {
                if (args.IsReConnected)
                {
                    args.MarkAsDisconnected();
                    await InvokeClientConnectionChangedMethod(args);
                }
            }
            else
            {
                _disconnectionCache.Add(_options.ClientId, MqttClientConnectionEventArgs.Disconnected(_options.ClientId, e.Reason));
                await InvokeClientConnectionChangedMethod(args);
            }

            await TryConnectAsync(true);
        }

        private Task InvokeClientConnectionChangedMethod(MqttClientConnectionEventArgs args)
        {
            var handler = ClientConnectionChanged;
            if (handler != null)
                return handler.Invoke(this, args);
            return Task.CompletedTask;
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
