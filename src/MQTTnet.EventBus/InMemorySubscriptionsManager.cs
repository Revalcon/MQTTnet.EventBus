using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTnet.EventBus
{
    public partial class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        private readonly Dictionary<string, HashSet<SubscriptionInfo>> _cache;
        private readonly HashSet<string> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public InMemorySubscriptionsManager()
        {
            _cache = new Dictionary<string, HashSet<SubscriptionInfo>>();
            _eventTypes = new HashSet<string>();
        }

        public bool IsEmpty => !_cache.Keys.Any();
        public void Clear() => _cache.Clear();

        public void AddSubscription(SubscriptionInfo subscriptionInfo)
        {
            string eventName = subscriptionInfo.EventType.Name;
            DoAddSubscription(subscriptionInfo);

            if (!_eventTypes.Contains(eventName))
            {
                _eventTypes.Add(eventName);
            }
        }

        //public void AddSubscription<TConsmer>(string topic)
        //    where TConsmer : IConsumer
        //{
        //    var eventName = topic;

        //    DoAddSubscription(typeof(TConsmer), eventName);

        //    if (!_eventTypes.Contains(eventName))
        //    {
        //        _eventTypes.Add(eventName);
        //    }
        //}

        private void DoAddSubscription(SubscriptionInfo subscriptionInfo)
        {
            string eventName = subscriptionInfo.Topic;
            if (!HasSubscriptionsForEvent(eventName))
            {
                _cache.Add(eventName, new HashSet<SubscriptionInfo>(ComparersManager.SubscriptionInfo));
            }

            var consumerType = subscriptionInfo.ConsumerType;
            if (_cache[eventName].Any(s => s.ConsumerType == consumerType))
            {
                throw new ArgumentException(
                    $"Handler Type {consumerType.Name} already registered for '{eventName}'", nameof(consumerType));
            }

            _cache[eventName].Add(subscriptionInfo);
        }

        public void RemoveSubscription(SubscriptionInfo subscriptionInfo)
        {
            var handlerToRemove = FindSubscriptionToRemove(subscriptionInfo.Topic, subscriptionInfo.ConsumerType);
            DoRemoveConsumer(subscriptionInfo.Topic, handlerToRemove);
        }

        //public void RemoveSubscription<TConsmer>(string topic)
        //    where TConsmer : IConsumer
        //{
        //    var handlerToRemove = FindSubscriptionToRemove<TConsmer>(topic);
        //    DoRemoveConsumer(topic, handlerToRemove);
        //}

        private void DoRemoveConsumer(string topic, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _cache[topic].Remove(subsToRemove);
                if (!_cache[topic].Any())
                {
                    _cache.Remove(topic);
                    var eventType = _eventTypes.SingleOrDefault(e => e == topic);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(topic);
                }
            }
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string topic) => _cache[topic];

        public IEnumerable<string> AllTopics() => _cache.Select(p => p.Key);

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove(string topic, Type consumerType)
        {
            if (!HasSubscriptionsForEvent(topic))
                return null;

            return _cache[topic].SingleOrDefault(s => s.ConsumerType == consumerType);
        }
        public bool HasSubscriptionsForEvent(string topic) => _cache.ContainsKey(topic);

        public HashSet<SubscriptionInfo> GetSubscriptions(string topic)
        {
            if (_cache.TryGetValue(topic, out var options))
                return options;
            return null;
        }
    }
}