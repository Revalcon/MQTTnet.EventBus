using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Reflection
{
    public interface IConsumeMethodInvoker
    {
        Task InvokeAsync(object consumer, Type eventType, object[] args);
    }

    public static class ConsumeMethodInvokerExtensions
    {
        public static Task InvokeAsync(this IConsumeMethodInvoker invoker, object consumer, Type eventType, object deserializer, MqttApplicationMessageReceivedEventArgs message)
            => invoker.InvokeAsync(consumer, eventType, new object[]  {
                (Serializers.IEventDeserializer<object>)deserializer,
                message
            });
    }
}
