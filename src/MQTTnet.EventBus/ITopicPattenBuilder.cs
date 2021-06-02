using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MQTTnet.EventBus
{
    public class TopicPatternInfo
    {
        public bool IsConst { get; set; }
        public string Name { get; set; }
    }

    public interface ITopicPattenBuilder
    {
        string GetTopicEntity(string topicPattern, string topic, string name);
        void SetData(object model, string topicPattern, string topic);
        Expression<Func<object, string>> CreateTopic(Type eventType, string topicPattern);
        Expression<Func<TEvent, string>> CreateTopic<TEvent>(string topicPattern);
        IEnumerable<TopicPatternInfo> Parse(string topicPattern);
        string ConvertToFormattedString(IEnumerable<TopicPatternInfo> patternInfos);
    }
}
