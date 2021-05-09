using System;

namespace MQTTnet.EventBus
{
    public interface ITopicComparer
    {
        bool IsMatch(string topic, string filter);
    }

    public class MqttTopicComparer : ITopicComparer
    {
        public bool IsMatch(string topic, string filter)
        {
            if (topic == null) 
                throw new ArgumentNullException(nameof(topic));

            return Server.MqttTopicFilterComparer.IsMatch(topic, filter);
        }
    }
}
