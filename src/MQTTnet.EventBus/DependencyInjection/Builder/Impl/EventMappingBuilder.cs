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

        public IEventMappingBuilder<TEvent> UseTopicPattern(string value)
        {
            _eventOptions.TopicPattern = value;
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

        public IEventMappingBuilder<TEvent> UseTopicPattern<TTopicInfo>(Expression<Func<TTopicInfo, string>> patternExp)
        {
            string pattern = ReflectionHelper.CreateTopicPattern(patternExp);
            _eventOptions.TopicInfoType = typeof(TTopicInfo);
            return UseTopicPattern(pattern);
        }
    }
}
