using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public class MessageBuilderInfo
    {
        public Type ConverterType { get; set; }
        public Action<MqttApplicationMessageBuilder> MessageCreater { get; set; }
    }
}
