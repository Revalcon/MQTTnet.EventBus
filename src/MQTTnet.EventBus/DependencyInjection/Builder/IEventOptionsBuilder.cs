using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IEventOptionsBuilder
    {
        IEventOptionsBuilder AddEventMapping<TEvent>(string eventName, Action<IEventMappingBuilder<TEvent>> mappingConfigurator);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using MQTTnet.EventBus.DependencyInjection.Builder;

    public static class EventOptionsBuilderExtensions
    {
        public static IEventOptionsBuilder AddEventMapping<TEvent>(this IEventOptionsBuilder builder, Action<IEventMappingBuilder<TEvent>> mappingConfigurator)
            => builder.AddEventMapping(typeof(TEvent).Name, mappingConfigurator);
    }
}
