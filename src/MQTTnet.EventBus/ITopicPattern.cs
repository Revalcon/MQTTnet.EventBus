using System;

namespace MQTTnet.EventBus
{
    public interface ITopicPattern<out TEvent>
    {
        Type EventType => typeof(TEvent);
    }
}
