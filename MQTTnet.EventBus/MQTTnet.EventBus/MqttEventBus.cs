using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using Polly;
using Polly.Retry;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public class MqttEventBus : IEventBus, IDisposable
    {
        private readonly IMqttPersisterConnection _persistentConnection;
        private readonly ILogger<MqttEventBus> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _retryCount;

        private IMqttClient _mqttClient;

        public MqttEventBus(IMqttPersisterConnection persistentConnection, ILogger<MqttEventBus> logger, 
            IServiceScopeFactory scopeFactory, IEventBusSubscriptionsManager subsManager, int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _mqttClient = _persistentConnection.GetClient();
            _mqttClient.UseApplicationMessageReceivedHandler(e => Consumer_Received(e));
            _retryCount = retryCount;
            _scopeFactory = scopeFactory;

            //_subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            //_mqttClient.UnsubscribeAsync()
        }

        private async void Consumer_Received(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            string topic = eventArgs?.ApplicationMessage?.Topic;
            try
            {
                _logger.LogTrace($"Processing Mqtt topic: {topic}");
                await ProcessEvent(eventArgs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"----- ERROR Processing topic \"{topic}\"");
            }
        }

        public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<MqttCommunicationException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, $"Could not publish topic: {message?.Topic} after {time.TotalSeconds:n1}s ({ex.Message})");
                });

            return policy.Execute(() => _mqttClient.PublishAsync(message));
        }

        private async Task ProcessEvent(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var message = eventArgs.ApplicationMessage;
            _logger.LogTrace($"Processing Mqtt topic: {message.Topic}");

            if (_subsManager.HasSubscriptionsForEvent(message.Topic))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(message.Topic);
                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null)
                            continue;

                        var eventType = _subsManager.GetEventType();
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                        await Task.Yield();
                        await (Task)concreteType
                            .GetMethod(nameof(IIntegrationEventHandler.Handle))
                            .Invoke(handler, new object[] { eventArgs });
                    }
                }
            }
            else
            {
                _logger.LogWarning($"No subscription for Mqtt topic: {message.Topic}");
            }
        }

        public void Dispose()
        {
            if (_mqttClient != null)
            {
                _mqttClient.Dispose();
            }

            _subsManager.Clear();
        }

        public Task<MqttClientSubscribeResult> SubscribeAsync<TH>(string topic)
            where TH : IIntegrationEventHandler
        {
            _logger.LogInformation($"Subscribing to topic {topic} with {typeof(TH).Name}");

            var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                _subsManager.AddSubscription<TH>(topic);
                return _mqttClient.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce);
            }

            return Task.Factory.StartNew(() => new MqttClientSubscribeResult());
        }

        public Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TH>(string topic) 
            where TH : IIntegrationEventHandler
        {
            _logger.LogInformation($"Subscribing to topic {topic} with {typeof(TH).Name}");

            var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                _subsManager.RemoveSubscription<TH>(topic);
                return _mqttClient.UnsubscribeAsync(topic);
            }

            return Task.Factory.StartNew(() => new MqttClientUnsubscribeResult());
        }
    }
}