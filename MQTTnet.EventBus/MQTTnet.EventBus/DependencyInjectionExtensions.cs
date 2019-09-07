using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
using System;

namespace MQTTnet.EventBus
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, Action<MqttClientOptionsBuilder> mqttClientOptionsAction, int retryCount = 5)
        {
            var builder = new MqttClientOptionsBuilder();
            mqttClientOptionsAction.Invoke(builder);
            var options = builder.Build();

            return services.AddMqttEventBus(options, retryCount);
        }

        public static IServiceCollection AddMqttEventBus(this IServiceCollection services, IMqttClientOptions mqttClientOptions, int retryCount = 5)
        {
            services.AddSingleton<IEventBus, MqttEventBus>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IMqttPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<MqttEventBus>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new MqttEventBus(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, retryCount);
            });
            services.AddSingleton<IMqttPersisterConnection, DefaultMqttPersisterConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultMqttPersisterConnection>>();
                return new DefaultMqttPersisterConnection(mqttClientOptions, logger);
            });
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            return services;
        }
    }
}