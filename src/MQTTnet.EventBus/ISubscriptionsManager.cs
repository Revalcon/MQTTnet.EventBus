using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public interface ISubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddSubscription(SubscriptionInfo subscriptionInfo);
        void RemoveSubscription(SubscriptionInfo subscriptionInfo);
        //void AddSubscription<TConsmer>(string topic)
        //   where TConsmer : IConsumer;
        //void RemoveSubscription<TConsmer>(string topic)
        //     where TConsmer : IConsumer;
        bool HasSubscriptionsForEvent(string topic);
        Type GetEventType();
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        IEnumerable<string> AllTopics();
    }
}