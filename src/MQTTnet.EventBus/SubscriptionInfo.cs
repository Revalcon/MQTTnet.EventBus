using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public class SubscriptionInfo
    {
        public string EventName { get; set; }
        public string Topic { get; set; }
        public Type EventType { get; set; }
        public Type ConsumerType { get; set; }
    }

    public class SubscriptionInfoEqualityComparer : IEqualityComparer<SubscriptionInfo>
    {
        public bool Equals(SubscriptionInfo x, SubscriptionInfo y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null)
                return false;
            if (y is null)
                return false;

            return x.EventType.Equals(y.EventType) && x.ConsumerType.Equals(y.ConsumerType);
        }

        public int GetHashCode(SubscriptionInfo obj)
        {
            unchecked { return obj.EventType.GetHashCode() ^ obj.ConsumerType.GetHashCode(); }
        }
    }
}