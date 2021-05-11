using MQTTnet.Diagnostics;
using System;

namespace MQTTnet.EventBus.Logger
{
    public interface IEventBusLogger : IMqttNetScopedLogger
    {
        IEventBusLogger CreateLogger(string categoryName);
        IEventBusLogger<TCategoryName> CreateLogger<TCategoryName>();

        void LogTrace(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogWarning(Exception ex, string message);
        void LogError(Exception ex, string message);
    }

    public class EventBusLogger : IEventBusLogger
    {
        private readonly string _source;
        private readonly IMqttNetLogger _logger;
        private readonly IMqttNetScopedLogger _scopedLogger;

        public EventBusLogger(IMqttNetLogger logger, string categoryName)
        {
            _source = categoryName;
            _logger = logger;
            _scopedLogger = CreateScopedLogger(_source);
        }

        public IEventBusLogger CreateLogger(string categoryName)
            => new EventBusLogger(_logger, categoryName);

        public IEventBusLogger<TCategoryName> CreateLogger<TCategoryName>()
            => new EventBusLogger<TCategoryName>(_logger);

        public IMqttNetScopedLogger CreateScopedLogger(string source)
            => _logger.CreateScopedLogger(source);

        public void LogError(Exception ex, string message)
            => _scopedLogger.Error(ex, message);

        public void LogInformation(string message)
            => _scopedLogger.Info(message);

        public void LogTrace(string message)
            => _scopedLogger.Verbose(message);

        public void LogWarning(string message)
            => _scopedLogger.Warning(message);

        public void LogWarning(Exception ex, string message)
            => _scopedLogger.Warning(ex, message);

        public void Publish(MqttNetLogLevel logLevel, string message, object[] parameters, Exception exception)
            => _logger.Publish(logLevel, _source, message, parameters, exception);
    }
}
