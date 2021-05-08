using MQTTnet.Client.Options;
using MQTTnet.EventBus;
using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.DependencyInjection.Builder.Impl;
using MQTTnet.EventBus.Reflection;
using MQTTnet.EventBus.Serializers.Default;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder, IEventOptionsBuilder> configurator, int retryCount = 5)
        {
            var builder = new MqttClientOptionsBuilder();
            var eventBuilder = new EventOptionsBuilder();
            configurator.Invoke(builder, eventBuilder);
            var options = builder.Build();
            
            var eventOptions = eventBuilder.Build();
            foreach (var o in eventOptions)
            {
                services.AddSingleton(o.ConsumerType);
                services.AddSingleton(o.ConverterType);
            }

            services.AddSingleton(p => {
                StaticCache.EventProvider = new EventProvider(p, eventOptions);
                return StaticCache.EventProvider;
            });
            return services.AddMqttEventBus(options, new BusOptions { RetryCount = retryCount });
        }

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder> hostConfigurator, int retryCount = 5)
        {
            var builder = new MqttClientOptionsBuilder();
            hostConfigurator.Invoke(builder);
            var options = builder.Build();

            return services.AddMqttEventBus(options, new BusOptions { RetryCount = retryCount });
        }

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, IMqttClientOptions mqttClientOptions, int retryCount = 5)
            => services.AddMqttEventBus(mqttClientOptions, new BusOptions { RetryCount = retryCount });

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, IMqttClientOptions mqttClientOptions, BusOptions busOptions)
            => services
                .AddLogging()
                .AddSingleton(busOptions)
                .AddSingleton(mqttClientOptions)
                .AddSingleton<IDefaultConverter, DefaultConverter>()
                .AddSingleton<IEventBus, MqttEventBus>()
                .AddSingleton<IMqttPersisterConnection, DefaultMqttPersisterConnection>()
                .AddSingleton<IEventBusClientProvider, EventBusClientProvider>()
                .AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>()
                .AddSingleton<IConsumeMethodInvoker, ConsumeMethodInvoker>();
    }
}