using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public interface ISubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        bool TryAddSubscription(SubscriptionInfo subscriptionInfo);
        void RemoveSubscription(SubscriptionInfo subscriptionInfo);
        bool HasSubscriptionsForEvent(string topic);
        HashSet<SubscriptionInfo> GetSubscriptions(string topic);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        IEnumerable<string> AllTopics();
    }
}