using System;
using System.Collections.Generic;
using System.Linq;
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
        Expression<Func<object, string>> CreateTopicCreater(Type eventType, string topicPattern);
        Expression<Func<TEvent, string>> CreateTopic<TEvent>(string topicPattern);
        IEnumerable<TopicPatternInfo> Parse(string topicPattern);
        string ConvertToFormattedString(IEnumerable<TopicPatternInfo> patternInfos);
    }

    public static class TopicPattenBuilderExtensions
    {
        public static bool IsPattern(this ITopicPattenBuilder topicPattenBuilder, string topicPattern)
            => topicPattenBuilder.Parse(topicPattern).Any(p => !p.IsConst);

        public static bool IsStaticTopic(this ITopicPattenBuilder topicPattenBuilder, string topicPattern)
            => topicPattenBuilder.Parse(topicPattern).All(p => p.IsConst);
    }
}
