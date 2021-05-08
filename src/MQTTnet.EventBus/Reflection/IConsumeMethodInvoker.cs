using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Reflection
{
    public interface IConsumeMethodInvoker
    {
        //Task InvokeAsync<TConsumer, TEvent>(TConsumer consumer, params object[] args);
        //Task InvokeAsync<TConsumer>(TConsumer consumer, Type eventType, params object[] args);
        Task InvokeAsync(object consumer, Type eventType, object[] args);
    }

    public static class ConsumeMethodInvokerExtensions
    {
        //public static Task InvokeAsync<TConsumer, TEvent>(this IConsumeMethodInvoker invoker, TConsumer consumer, IEventDeserializer<TEvent> deserializer, MqttApplicationMessageReceivedEventArgs message)
        //    => invoker.InvokeAsync<TConsumer, TEvent>(consumer, new object[] { deserializer, message });

        //public static Task InvokeAsync<TConsumer, TEvent>(this IConsumeMethodInvoker invoker, TConsumer consumer, Lazy<TEvent> converter, MqttApplicationMessageReceivedEventArgs message)
        //    => invoker.InvokeAsync<TConsumer, TEvent>(consumer, new object[] { converter, message });

        public static Task InvokeAsync(this IConsumeMethodInvoker invoker, object consumer, Type eventType, Type deserializerType, MqttApplicationMessageReceivedEventArgs message)
        {
            var deserializer = Activator.CreateInstance(deserializerType.MakeGenericType(eventType));
            return invoker.InvokeAsync(consumer, eventType, new object[] { deserializer, message });
        }

        public static Task InvokeAsync<TEvent>(this IConsumeMethodInvoker invoker, object consumer, Lazy<TEvent> converter, MqttApplicationMessageReceivedEventArgs message)
            => invoker.InvokeAsync(consumer, typeof(TEvent), new object[] { converter, message });
    }
}
