using System.Collections.Generic;

namespace MQTTnet.EventBus
{
    public static class ComparersManager
    {
        public static IEqualityComparer<SubscriptionInfo> SubscriptionInfo { get; } = new SubscriptionInfoEqualityComparer();
        public static IEqualityComparer<EventOptions> EventOptions { get; } = new EventOptionsEqualityComparer();
    }
}
