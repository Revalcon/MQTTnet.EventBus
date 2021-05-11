using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using MQTTnet.EventBus.Logger;

namespace MQTTnet.EventBus.Microsoft.Extensions.Logging
{
    public class MicrosoftEventBusLogger : IEventBusLogger
    {
        protected readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private static readonly IDictionary<MqttNetLogLevel, LogLevel> _logLevelMap;

        static MicrosoftEventBusLogger()
        {
            _logLevelMap = new Dictionary<MqttNetLogLevel, LogLevel>
            {
                { MqttNetLogLevel.Verbose, LogLevel.Trace },
                { MqttNetLogLevel.Info, LogLevel.Information },
                { MqttNetLogLevel.Warning, LogLevel.Warning },
                { MqttNetLogLevel.Error, LogLevel.Error }
            };
        }

        public MicrosoftEventBusLogger(ILoggerFactory loggerFactory, ILogger logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public IEventBusLogger CreateLogger(string categoryName)
            => new MicrosoftEventBusLogger(_loggerFactory, _loggerFactory.CreateLogger(categoryName));

        public IMqttNetScopedLogger CreateScopedLogger(string source)
            => new MicrosoftEventBusLogger(_loggerFactory, _loggerFactory.CreateLogger(source));

        public IEventBusLogger<TCategoryName> CreateLogger<TCategoryName>()
            => new MicrosoftEventBusLogger<TCategoryName>(_loggerFactory, _loggerFactory.CreateLogger<TCategoryName>());

        public void LogTrace(string message)
            => _logger.LogTrace(message);

        public void LogInformation(string message)
            => _logger.LogInformation(message);

        public void LogWarning(string message)
            => _logger.LogWarning(message);

        public void LogWarning(Exception ex, string message)
            => _logger.LogWarning(ex, message);

        public void LogError(Exception ex, string message)
            => _logger.LogError(ex, message);

        public void Publish(MqttNetLogLevel logLevel, string message, object[] parameters, Exception exception)
        {
            var mqttNetLogLevel = _logLevelMap[logLevel];
            _logger.Log(mqttNetLogLevel, exception, message, parameters);
        }
    }

    public class MicrosoftEventBusLogger<TCategoryName> : MicrosoftEventBusLogger, IEventBusLogger<TCategoryName>
    {
        public MicrosoftEventBusLogger(ILoggerFactory loggerFactory, ILogger<TCategoryName> logger)
            : base(loggerFactory, logger)
        { }
    }
}
