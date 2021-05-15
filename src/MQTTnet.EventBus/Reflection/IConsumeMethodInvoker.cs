using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Reflection
{
    public interface IConsumeMethodInvoker
    {
        Task InvokeAsync(IServiceProvider serviceProvider, SubscriptionInfo subscriptionInfo, MqttApplicationMessageReceivedEventArgs messageReceived);
    }
}
