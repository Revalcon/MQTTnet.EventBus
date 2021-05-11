using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.EventBus.Logger;
using MQTTnet.EventBus.Reflection;
using MQTTnet.Exceptions;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public class MqttEventBus : IEventBus, IDisposable
    {
        private readonly IEventBusLogger<MqttEventBus> _logger;
        private readonly ISubscriptionsManager _subsManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConsumeMethodInvoker _consumeMethodInvoker;
        private readonly IEventBusClientProvider _eventBusClientProvider;
        private readonly IEventProvider _eventProvider;
        private readonly int _retryCount;

        public MqttEventBus(
            IEventBusClientProvider eventBusClientProvider,
            IEventProvider eventProvider,
            IConsumeMethodInvoker consumeMethodInvoker,
            IEventBusLogger<MqttEventBus> logger,
            IServiceScopeFactory scopeFactory,
            ISubscriptionsManager subsManager,
            BusOptions busOptions)
        {
            _eventBusClientProvider = eventBusClientProvider ?? throw new ArgumentNullException(nameof(eventBusClientProvider));
            _eventProvider = eventProvider;
            _consumeMethodInvoker = consumeMethodInvoker;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? throw new ArgumentNullException(nameof(ISubscriptionsManager));
            _retryCount = busOptions?.RetryCount ?? 5;
            _scopeFactory = scopeFactory;
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
            var connection = _eventBusClientProvider.GetOrCreateMqttConnection(message.Topic);
            if (!connection.IsConnected)
            {
                connection.TryConnect();
            }

            var policy = RetryPolicy.Handle<SocketException>()
                .Or<MqttCommunicationException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, $"Could not publish topic: {message?.Topic} after {time.TotalSeconds:n1}s ({ex.Message})");
                });

            return policy.Execute(() => connection.GetClient().PublishAsync(message));
        }

        private async Task ProcessEvent(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var message = eventArgs.ApplicationMessage;
            _logger.LogTrace($"Processing Mqtt topic: {message.Topic}");

            var subscriptions = _subsManager.GetSubscriptions(message.Topic);
            if (subscriptions.IsNullOrEmpty())
            {
                _logger.LogWarning($"No subscription for Mqtt topic: {message.Topic}");
            }
            else
            {
                foreach (var subscription in subscriptions)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var consumer = scope.ServiceProvider.GetService(subscription.ConsumerType);
                        if (consumer == null)
                            continue;

                        var converterType = _eventProvider.GetConverterType(subscription.EventName);
                        var converter = scope.ServiceProvider.GetService(converterType);
                        await Task.Yield();
                        await _consumeMethodInvoker.InvokeAsync(consumer, subscription.EventType, converter, eventArgs);
                    }
                }
            }
        }

        public void Dispose()
        {
            _eventBusClientProvider.Dispose();
            _subsManager.Clear();
        }

        public Task<MqttClientSubscribeResult> SubscribeAsync(SubscriptionInfo subscriptionInfo)
        {
            string topic = subscriptionInfo.Topic;
            _logger.LogInformation($"Subscribing to topic {topic} with {subscriptionInfo.ConsumerType.Name}");

            var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
            if (!containsKey)
            {
                if (_eventBusClientProvider.RegisterMessageHandler(topic, Consumer_Received, out var connection))
                {
                    _subsManager.AddSubscription(subscriptionInfo);
                    return connection.GetClient().SubscribeAsync(topic);
                }
            }

            return Task.Factory.StartNew(() => new MqttClientSubscribeResult());
        }

        public Task<MqttClientUnsubscribeResult> UnsubscribeAsync(SubscriptionInfo subscriptionInfo)
        {
            var topic = subscriptionInfo.Topic;
            _logger.LogInformation($"Subscribing to topic {topic} with {subscriptionInfo.ConsumerType.Name}");

            var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
            if (!containsKey)
            {
                _subsManager.RemoveSubscription(subscriptionInfo);
                return _eventBusClientProvider.RemoveSubscriptionAsync(topic);
            }

            return Task.Factory.StartNew(() => new MqttClientUnsubscribeResult());
        }


        //public Task<MqttClientSubscribeResult> SubscribeAsync<TConsumer>(string topic)
        //    where TConsumer : IConsumer
        //{
        //    _logger.LogInformation($"Subscribing to topic {topic} with {typeof(TConsumer).Name}");

        //    var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
        //    if (!containsKey)
        //    {
        //        if (_eventBusClientProvider.RegisterMessageHandler(topic, Consumer_Received, out var connection))
        //        {
        //            _subsManager.AddSubscription<TConsumer>(topic);
        //            return connection.GetClient().SubscribeAsync(topic);
        //        }
        //    }

        //    return Task.Run(() => new MqttClientSubscribeResult());
        //}

        public Task<MqttClientSubscribeResult[]> ReSubscribeAllTopicsAsync()
        {
            var aaa = _subsManager.AllTopics().Select(topic =>
            {
                var connection = _eventBusClientProvider.GetOrCreateMqttConnection(topic);
                if (!connection.IsConnected)
                {
                    if (connection.TryConnect())
                    {
                        return connection.GetClient().SubscribeAsync(topic);
                    }
                    return Task.Run(() => new MqttClientSubscribeResult());
                }
                else
                    return connection.GetClient().SubscribeAsync(topic);
            });

            return Task.WhenAll(aaa);
        }

        //public Task<MqttClientUnsubscribeResult> UnsubscribeAsync<TConsumer>(string topic)
        //    where TConsumer : IConsumer
        //{
        //    _logger.LogInformation($"Subscribing to topic {topic} with {typeof(TConsumer).Name}");

        //    var containsKey = _subsManager.HasSubscriptionsForEvent(topic);
        //    if (!containsKey)
        //    {
        //        _subsManager.RemoveSubscription<TConsumer>(topic);
        //        return _eventBusClientProvider.RemoveSubscriptionAsync(topic);
        //    }

        //    return Task.Factory.StartNew(() => new MqttClientUnsubscribeResult());
        //}
    }
}