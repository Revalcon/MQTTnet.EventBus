using System;
using System.Linq.Expressions;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IEventMappingBuilder<TEvent>
    {
        IEventMappingBuilder<TEvent> AddConsumer<TConsumer>() where TConsumer : IConsumer<TEvent>;
        IEventMappingBuilder<TEvent> UseConverter<TConverter>() where TConverter : Serializers.IEventConverter<TEvent>;
        IEventMappingBuilder<TEvent> UseTopicPattern(string value);
        IEventMappingBuilder<TEvent> UseTopicPattern<TTopicInfo>(Expression<Func<TTopicInfo, string>> patternExp);
        IEventMappingBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator);
    }
}
