using System;
using Microsoft.Extensions.Logging;
using MQTTnet.EventBus.DependencyInjection;
using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MicrosoftLoggerOptionsBuilderExtensions
    {
        public static IServicesBuilder AddMicrosoftExtensionsLgger(this IServicesBuilder builder)
            => builder.AddLogger(p => p.UseMicrosoftExtension());

        public static IServicesBuilder AddMicrosoftExtensionsLgger(this IServicesBuilder builder, Action<ILoggingBuilder> configure)
            => builder.AddLogger(p => p.UseMicrosoftExtension(configure));

        public static ILoggerOptionsBuilder UseMicrosoftExtension(this ILoggerOptionsBuilder builder)
        {
            builder.Services
                .AddLogging()
                .AddEventBusLogger<MicrosoftEventBusLogger>();
            return builder;
        }

        public static ILoggerOptionsBuilder UseMicrosoftExtension(this ILoggerOptionsBuilder builder, Action<ILoggingBuilder> configure)
        {
            builder.Services
                .AddLogging(configure)
                .AddEventBusLogger<MicrosoftEventBusLogger>();
            return builder;
        }
    }
}
