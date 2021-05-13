using MQTTnet.EventBus.DependencyInjection.Builder;
using MQTTnet.EventBus.Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftJsonServiceCollectionExtensions
    {
        public static IMessageBuilder<TEvent> UseJsonConverter<TEvent>(this IMessageBuilder<TEvent> builder)
        {
            builder.UseConverter<NewtonsoftJsonConverter<TEvent>>();
            return builder;
        }
    }
}
