using MQTTnet.EventBus.Serializers;
using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder.Impl
{
    public class MessageBuilder<TEvent> : IMessageBuilder<TEvent>
    {
        private readonly EventOptions _consumerOptions;

        public MessageBuilder(EventOptions consumerOptions)
        {
            _consumerOptions = consumerOptions;
        }

        public IMessageBuilder<TEvent> UseConverter<TConverter>() where TConverter : IEventConverter<TEvent>
        {
            _consumerOptions.ConverterType = typeof(TConverter);
            return this;
        }

        public IMessageBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator)
        {
            _consumerOptions.MessageCreater = messageBuilderConfigurator;
            return this;
        }
    }
}
