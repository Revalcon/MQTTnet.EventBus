namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    using Microsoft.Extensions.DependencyInjection;

    public interface ILoggerOptionsBuilder
    {
        IServiceCollection Services { get; }
    }

    public class LoggerOptionsBuilder : ILoggerOptionsBuilder
    {
        public LoggerOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using MQTTnet.Diagnostics;
    using MQTTnet.EventBus.DependencyInjection;
    using MQTTnet.EventBus.DependencyInjection.Builder;
    using MQTTnet.EventBus.Logger;

    public static class InnerLoggerOptionsBuilderExtensions
    {
        public static ILoggerOptionsBuilder UseInnerLogger(this ILoggerOptionsBuilder builder)
        {
            builder.Services
                .AddSingleton<IMqttNetLogger, MqttNetLogger>()
                .AddEventBusLogger<EventBusLogger>();
            return builder;
        }
    }
}