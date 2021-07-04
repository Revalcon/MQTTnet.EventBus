using MQTTnet.EventBus.Reflection;
using MQTTnet.EventBus.Serializers;
using System;
using System.Linq.Expressions;

namespace MQTTnet.EventBus.DependencyInjection.Builder.Impl
{
    public class EventMappingBuilder<TEvent> : IEventMappingBuilder<TEvent>
    {
        private readonly EventOptions _eventOptions;

        public EventMappingBuilder(EventOptions eventOptions)
        {
            _eventOptions = eventOptions;
        }

        public IEventMappingBuilder<TEvent> AddConsumer<TConsumer>()
            where TConsumer : IConsumer<TEvent>
        {
            _eventOptions.ConsumerType = typeof(TConsumer);
            return this;
        }

        public IEventMappingBuilder<TEvent> UseConverter<TConverter>()
            where TConverter : IEventConverter<TEvent>
        {
            _eventOptions.ConverterType = typeof(TConverter);
            return this;
        }

        public IEventMappingBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator)
        {
            _eventOptions.MessageCreater = messageBuilderConfigurator;
            return this;
        }

        public IEventMappingBuilder<TEvent> UseTopicPattern(string pattern) =>
            UseTopicPattern(pattern, pattern);

        public IEventMappingBuilder<TEvent> UseTopicPattern(string root, string pattern)
        {
            _eventOptions.Topic.Root = root;
            _eventOptions.Topic.Pattern = pattern;
            return this;
        }

        public IEventMappingBuilder<TEvent> UseTopicPattern<TPatternType>(string root, Expression<Func<TPatternType, string>> patternExp)
            where TPatternType : ITopicPattern<TEvent>
        {
            string pattern = ReflectionHelper.CreateTopicPattern(patternExp);
            _eventOptions.Topic.PatternType = typeof(TPatternType);

            if (!string.IsNullOrWhiteSpace(root))
            {
                if (!pattern.StartsWith(root))
                {
                    root = root.TrimEnd('/');
                    pattern = pattern.TrimStart('/');
                    pattern = $"{root}/{pattern}";
                }
            }

            return UseTopicPattern(root, pattern);
        }
    }
}
