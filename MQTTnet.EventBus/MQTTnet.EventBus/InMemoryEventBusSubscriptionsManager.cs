using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTnet.EventBus
{
    public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<string> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<string>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddSubscription<TH>(string topic)
            where TH : IIntegrationEventHandler
        {
            var eventName = topic;

            DoAddSubscription(typeof(TH), eventName);

            if (!_eventTypes.Contains(eventName))
            {
                _eventTypes.Add(eventName);
            }
        }

        private void DoAddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }

        public void RemoveSubscription<TH>(string topic)
            where TH : IIntegrationEventHandler
        {
            var handlerToRemove = FindSubscriptionToRemove<TH>(topic);
            DoRemoveHandler(topic, handlerToRemove);
        }

        private void DoRemoveHandler(string topic, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[topic].Remove(subsToRemove);
                if (!_handlers[topic].Any())
                {
                    _handlers.Remove(topic);
                    var eventType = _eventTypes.SingleOrDefault(e => e == topic);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(topic);
                }
            }
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string topic) => _handlers[topic];

        public IEnumerable<string> AllTopics() => _handlers.Select(p => p.Key);

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<TH>(string topic)
             where TH : IIntegrationEventHandler
        {
            return DoFindSubscriptionToRemove(topic, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
                return null;

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }
        public bool HasSubscriptionsForEvent(string topic) => _handlers.ContainsKey(topic);

        public Type GetEventType() => typeof(MqttApplicationMessageReceivedEventArgs);
    }
}