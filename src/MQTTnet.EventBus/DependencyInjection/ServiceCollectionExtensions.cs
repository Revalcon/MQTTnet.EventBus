using MQTTnet.Client.Options;
using MQTTnet.EventBus;
using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.DependencyInjection.Builder.Impl;
using MQTTnet.EventBus.Reflection;
using MQTTnet.EventBus.Serializers.String;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class TempreryData
    {
        public bool hasLogger;
    }

    public static class ServiceCollectionExtensions
    {
        public static IServicesBuilder AddEvenets(this IServicesBuilder serviceBuilder, Action<IEventOptionsBuilder> configurator)
        {
            return serviceBuilder.AddServices(services => {
                var eventBuilder = new EventOptionsBuilder();
                configurator.Invoke(eventBuilder);

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
            }, ServiceType.Event);
        }

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder, IServicesBuilder> configurator, int retryCount = 5)
            => AddMqttEventBus(services, configurator, new BusOptions { RetryCount = retryCount });

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder, IServicesBuilder> configurator, BusOptions busOptions)
        {
            var addedServices = new HashSet<ServiceType>();

            var mqttHostBuilder = new MqttClientOptionsBuilder();
            var serviceBuilder = new ServicesBuilder(services, addedServices);

            configurator.Invoke(mqttHostBuilder, serviceBuilder);

            if (!addedServices.Contains(ServiceType.Logger))
            {
                serviceBuilder.AddLogger(cfg => cfg.UseInnerLogger());
            }

            return services
                .AddSingleton(mqttHostBuilder.Build())
                .AddSingleton(busOptions)
                .AddEventBusServices();
        }

        private static IServiceCollection AddEventBusServices(this IServiceCollection services)
            => services
                .AddSingleton<IStringConverter, StringConverter>()
                .AddSingleton<IEventBus, MqttEventBus>()
                .AddSingleton<ITopicComparer, MqttTopicComparer>()
                .AddSingleton<IMqttPersisterConnection, DefaultMqttPersisterConnection>()
                .AddSingleton<IEventBusClientProvider, EventBusClientProvider>()
                .AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>()
                .AddSingleton<IConsumeMethodInvoker, ConsumeMethodInvoker>();

        public static IServicesBuilder AddLogger(this IServicesBuilder builder, Action<ILoggerOptionsBuilder> loggerConfigurator)
        {
            return builder.AddServices(services => {
                var loggerBuilder = new LoggerOptionsBuilder(services);
                loggerConfigurator.Invoke(loggerBuilder);
            }, ServiceType.Logger);
        }
    }
}