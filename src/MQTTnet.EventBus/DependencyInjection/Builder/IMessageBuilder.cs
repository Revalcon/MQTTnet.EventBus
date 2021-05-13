using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IMessageBuilder<TEvent>
    {
        IMessageBuilder<TEvent> UseConverter<TConverter>() where TConverter : Serializers.IEventConverter<TEvent>;
        IMessageBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using MQTTnet.EventBus.DependencyInjection.Builder;
    using MQTTnet.EventBus.Serializers.Text;

    public static class IMessageBuilderExtensions
    {
        public static IMessageBuilder<string> UseTextConverter(this IMessageBuilder<string> messageBuilder)
            => messageBuilder.UseConverter<StringConverter>();
    }
}
