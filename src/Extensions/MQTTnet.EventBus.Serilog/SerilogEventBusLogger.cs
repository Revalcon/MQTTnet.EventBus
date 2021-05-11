using System;
using Serilog;
using MQTTnet.EventBus.Logger;
using MQTTnet.Diagnostics;
using System.Collections.Generic;
using Serilog.Events;

namespace MQTTnet.EventBus.Serilog
{
    public class SerilogEventBusLogger : IEventBusLogger
    {
        public const string CategoryTemplateName = "Category";
        protected readonly ILogger _logger;
        private static readonly IDictionary<MqttNetLogLevel, LogEventLevel> _logLevelMap;

        static SerilogEventBusLogger()
        {
            _logLevelMap = new Dictionary<MqttNetLogLevel, LogEventLevel>
            {
                { MqttNetLogLevel.Verbose, LogEventLevel.Verbose },
                { MqttNetLogLevel.Info, LogEventLevel.Information },
                { MqttNetLogLevel.Warning, LogEventLevel.Warning },
                { MqttNetLogLevel.Error, LogEventLevel.Error }
            };
        }

        public SerilogEventBusLogger(ILogger logger)
        {
            _logger = logger;
        }

        public IEventBusLogger CreateLogger(string categoryName)
            => new SerilogEventBusLogger(_logger.ForContext(CategoryTemplateName, categoryName));

        public IEventBusLogger<TCategoryName> CreateLogger<TCategoryName>()
            => new SerilogEventBusLogger<TCategoryName>(_logger.ForContext(CategoryTemplateName, typeof(TCategoryName).FullName));

        public IMqttNetScopedLogger CreateScopedLogger(string source)
            => CreateLogger(source);

        public void LogTrace(string message)
            => _logger.Verbose(message);

        public void LogInformation(string message)
            => _logger.Information(message);

        public void LogWarning(string message)
            => _logger.Warning(message);

        public void LogWarning(Exception ex, string message)
            => _logger.Information(ex, message);

        public void LogError(Exception ex, string message)
            => _logger.Error(ex, message);

        public void Publish(MqttNetLogLevel logLevel, string message, object[] parameters, Exception exception)
            => _logger.Write(_logLevelMap[logLevel], exception, message, parameters);
    }

    public class SerilogEventBusLogger<TCategoryName> : SerilogEventBusLogger, IEventBusLogger<TCategoryName>
    {
        public SerilogEventBusLogger(ILogger logger)
            : base(logger.ForContext(CategoryTemplateName, typeof(TCategoryName).FullName))
        { }
    }
}
