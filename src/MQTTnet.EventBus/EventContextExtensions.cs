using System;

namespace MQTTnet.EventBus
{
    public static class EventContextExtensions
    {

        public static string GetTopicEntity(this EventContext context, string name)
            => context.GetTopicInfo((provider, eventType) => provider.GetTopicEntity(eventType, context.Message.Topic, name));

        public static TResult GetTopicInfo<TResult>(this EventContext context, Func<IEventProvider, Type, TResult> func)
        {
            var contextType = context.GetType();
            if (!contextType.IsGenericType)
                return default;

            var eventType = contextType.GetGenericArguments()[0];
            if (StaticCache.EventProvider.HasTopicPattern(eventType))
            {
                return func.Invoke(StaticCache.EventProvider, eventType);
            }

            return default;
        }
    }
}
