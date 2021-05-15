using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftJsonServiceCollectionExtensions
    {
        public static IEventMappingBuilder<TEvent> UseJsonConverter<TEvent>(this IEventMappingBuilder<TEvent> builder)
        {
            builder.UseConverter<NewtonsoftJsonConverter<TEvent>>();
            return builder;
        }
    }
}
