using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddSubscription<TH>(string topic)
           where TH : IIntegrationEventHandler;
        void RemoveSubscription<TH>(string topic)
             where TH : IIntegrationEventHandler;
        bool HasSubscriptionsForEvent(string topic);
        Type GetEventType();
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        IEnumerable<string> AllTopics();
    }
}