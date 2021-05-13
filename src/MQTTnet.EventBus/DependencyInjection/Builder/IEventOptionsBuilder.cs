using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IEventOptionsBuilder
    {
        IEventOptionsBuilder AddConsumer<TEvent, TConsumer>(string eventName, Action<IMessageBuilder<TEvent>> convertorConfigurator) 
            where TConsumer : IConsumer<TEvent>;
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using MQTTnet.EventBus;
    using MQTTnet.EventBus.DependencyInjection.Builder;

    public static class IEventOptionsBuilderExtensions
    {
        public static IEventOptionsBuilder AddConsumer<TEvent, TConsumer>(this IEventOptionsBuilder builder, Action<IMessageBuilder<TEvent>> convertorConfigurator)
            where TConsumer : IConsumer<TEvent>
            => builder.AddConsumer<TEvent, TConsumer>(typeof(TEvent).Name, convertorConfigurator);

        public static IEventOptionsBuilder AddConsumer<TConsumer>(this IEventOptionsBuilder builder, string eventName, Action<IMessageBuilder<string>> convertorConfigurator = null)
                where TConsumer : IConsumer<string>
        {
            if(convertorConfigurator == null)
                convertorConfigurator = cfg => cfg.UseTextConverter();

            return builder.AddConsumer<string, TConsumer>(eventName, convertorConfigurator);
        }
    }
}
