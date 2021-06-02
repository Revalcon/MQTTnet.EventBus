using System;

namespace MQTTnet.EventBus
{
    public static class EventContextExtensions
    {
        public static T GetTopicInfo<T>(this EventContext context) where T : new()
        {
            var topicInfo = new T();
            bool created = context.InnerGet((provider, eventType) => provider.TrySetTopicInfo(topicInfo, eventType, context.Message.Topic));
            return created ? topicInfo : default;
        }

        public static string GetTopicEntity(this EventContext context, string name)
            => context.InnerGet((provider, eventType) => provider.GetTopicEntity(eventType, context.Message.Topic, name));

        private static TResult InnerGet<TResult>(this EventContext context, Func<IEventProvider, Type, TResult> func)
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
