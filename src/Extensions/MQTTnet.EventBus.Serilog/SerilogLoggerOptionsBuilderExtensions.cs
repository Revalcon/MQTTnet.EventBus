using System;
using Serilog;
using MQTTnet.EventBus.Serilog;
using MQTTnet.EventBus.DependencyInjection;
using MQTTnet.EventBus.DependencyInjection.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SerilogLoggerOptionsBuilderExtensions
    {
        public static IServicesBuilder AddSerilog(this IServicesBuilder builder)
           => builder.AddLogger(p => p.UseSerilog());

        public static IServicesBuilder AddSerilog(this IServicesBuilder builder, Action<LoggerConfiguration> configure)
           => builder.AddLogger(p => p.UseSerilog(configure));

        public static IServicesBuilder AddSerilog(this IServicesBuilder builder, ILogger logger)
           => builder.AddLogger(p => p.UseSerilog(logger));

        public static ILoggerOptionsBuilder UseSerilog(this ILoggerOptionsBuilder builder, Action<LoggerConfiguration> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var cfg = new LoggerConfiguration();
            configure.Invoke(cfg);
            return UseSerilog(builder, cfg.CreateLogger());
        }

        public static ILoggerOptionsBuilder UseSerilog(this ILoggerOptionsBuilder builder)
            => UseSerilog(builder, cfg => cfg.CreateLogger());

        public static ILoggerOptionsBuilder UseSerilog(this ILoggerOptionsBuilder builder, ILogger logger)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            Log.Logger = logger;
            builder.Services
                .AddSingleton(logger)
                .AddEventBusLogger<SerilogEventBusLogger>();
            return builder;
        }
    }
}
