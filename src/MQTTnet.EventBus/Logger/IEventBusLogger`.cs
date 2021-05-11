using MQTTnet.Diagnostics;

namespace MQTTnet.EventBus.Logger
{
    public interface IEventBusLogger<TCategoryName> : IEventBusLogger { }

    public class EventBusLogger<TCategoryName> : EventBusLogger, IEventBusLogger<TCategoryName>
    {
        public EventBusLogger(IMqttNetLogger logger) : base(logger, typeof(TCategoryName).FullName) { }
    }
}
