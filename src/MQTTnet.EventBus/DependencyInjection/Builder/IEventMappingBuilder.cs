using System;
using System.Linq.Expressions;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IEventMappingBuilder<TEvent>
    {
        IEventMappingBuilder<TEvent> AddConsumer<TConsumer>() where TConsumer : IConsumer<TEvent>;
        IEventMappingBuilder<TEvent> UseConverter<TConverter>() where TConverter : Serializers.IEventConverter<TEvent>;
        IEventMappingBuilder<TEvent> UseTopicPattern(string pattern);
        IEventMappingBuilder<TEvent> UseTopicPattern(string root, string pattern);
        IEventMappingBuilder<TEvent> UseTopicPattern<TTopicInfo>(string root, Expression<Func<TTopicInfo, string>> pattern)
            where TTopicInfo : ITopicPattern<TEvent>;
        IEventMappingBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator);
    }
}
