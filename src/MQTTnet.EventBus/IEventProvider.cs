using System;

namespace MQTTnet.EventBus
{
    public interface IEventProvider
    {
        bool TryGetEventName(Type eventType, out string eventName);
        bool HasTopicPattern(string eventName);
        bool TrySetTopicInfo(string eventName, object @event, string topic);
        string GetTopic(string eventName, object topicInfo);
        string GetTopicEntity(string eventName, string topic, string name);
        Type GetConverterType(string eventName);
        Type GetConsumerType(string eventName);
        MqttApplicationMessage CreateMessage(string eventName, object @event, string topic);
    }
}
