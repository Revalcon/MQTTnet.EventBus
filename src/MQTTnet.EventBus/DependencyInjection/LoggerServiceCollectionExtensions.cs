using Microsoft.Extensions.DependencyInjection;
using MQTTnet.EventBus.Logger;
using System;

namespace MQTTnet.EventBus.DependencyInjection
{
    public static class LoggerServiceCollectionExtensions
    {
        public static IServiceCollection AddEventBusLogger<TLogger>(this IServiceCollection services)
            where TLogger : IEventBusLogger
            => services.AddEventBusLogger(MakeGenericType<TLogger>());

        public static IServiceCollection AddEventBusLogger(this IServiceCollection services, Type loggerType)
            => services.AddSingleton(typeof(IEventBusLogger<>), loggerType);

        private static Type MakeGenericType<TLogger>() where TLogger : IEventBusLogger
        {
            var generycSymbol = typeof(IEventBusLogger<>).FullName[typeof(IEventBusLogger).FullName.Length..];
            var loggerType = typeof(TLogger);
            return loggerType.Assembly.GetType($"{loggerType.FullName}{generycSymbol}");
        }
    }
}
