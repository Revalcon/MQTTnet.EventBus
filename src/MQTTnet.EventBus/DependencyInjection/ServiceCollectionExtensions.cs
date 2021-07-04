using MQTTnet.Client.Options;
using MQTTnet.EventBus;
using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.DependencyInjection.Builder.Impl;
using MQTTnet.EventBus.Impl;
using MQTTnet.EventBus.Logger;
using MQTTnet.EventBus.Reflection;
using MQTTnet.EventBus.Serializers.Text;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServicesBuilder AddEvenets(this IServicesBuilder serviceBuilder, Action<IEventOptionsBuilder> configurator) =>
            serviceBuilder.AddServices(services =>
            {
                var eventBuilder = new EventOptionsBuilder();
                configurator.Invoke(eventBuilder);

                var eventOptions = eventBuilder.Build();
                foreach (var o in eventOptions)
                {
                    if (!o.ConsumerType.IsInterface)
                        services.AddScoped(o.ConsumerType);

                    if (!o.ConverterType.IsInterface)
                        services.AddScoped(o.ConverterType);
                }

                services.AddSingleton(serviceProvider =>
                {
                    StaticCache.EventProvider = new EventProvider(
                        serviceProvider,
                        serviceProvider.GetRequiredService<ITopicPattenBuilder>(),
                        eventOptions,
                        serviceProvider.GetRequiredService<IEventBusLogger<EventProvider>>());
                    return StaticCache.EventProvider;
                });
            }, ServiceType.Event);

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder, ILocalServerOptionsBuilder, IServicesBuilder> configurator) => 
            services.AddMqttEventBus(null, configurator);

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder, IServicesBuilder> configurator) => 
            services.AddMqttEventBus(configurator, null);

        private static IServiceCollection AddMqttEventBus(this IServiceCollection services, 
            Action<MqttClientOptionsBuilder, IServicesBuilder> cfgStrategy1, 
            Action<MqttClientOptionsBuilder, ILocalServerOptionsBuilder, IServicesBuilder> cfgStrategy2)
        {
            var addedServices = new HashSet<ServiceType>();

            var mqttHostBuilder = new MqttClientOptionsBuilder();
            var serviceBuilder = new ServicesBuilder(services, addedServices);
            var localServerBuilder = new LocalServerOptionsBuilder();
            
            if (cfgStrategy1 != null)
                cfgStrategy1.Invoke(mqttHostBuilder, serviceBuilder);

            if (cfgStrategy2 != null)
                cfgStrategy2.Invoke(mqttHostBuilder, localServerBuilder, serviceBuilder);

            if (!addedServices.Contains(ServiceType.Logger))
            {
                serviceBuilder.AddLogger(cfg => cfg.UseInnerLogger());
            }

            return services
                .AddSingleton(mqttHostBuilder.Build())
                .AddSingleton(localServerBuilder.Build())
                .AddEventBusServices();
        }

        private static IServiceCollection AddEventBusServices(this IServiceCollection services) => 
            services
                .AddSingleton<IStringConverter, StringConverter>()
                .AddSingleton<IEventBus, MqttEventBus>()
                .AddSingleton<ITopicComparer, MqttTopicComparer>()
                .AddSingleton<IMqttPersisterConnection, DefaultMqttPersisterConnection>()
                .AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>()
                .AddSingleton<IConsumeMethodInvoker, ConsumeMethodInvoker>()
                .AddSingleton<ITopicPattenBuilder, TopicPattenBuilder>();

        public static IServicesBuilder AddLogger(this IServicesBuilder builder, Action<ILoggerOptionsBuilder> loggerConfigurator) =>
            builder.AddServices(services =>
            {
                var loggerBuilder = new LoggerOptionsBuilder(services);
                loggerConfigurator.Invoke(loggerBuilder);
            }, ServiceType.Logger);

        public static IServiceCollection AddConsumer<TIConsumer, TConsumer>(this IServiceCollection services)
            where TIConsumer : class, IConsumer
            where TConsumer : class, TIConsumer
            => services.AddScoped<TIConsumer, TConsumer>();
    }
}