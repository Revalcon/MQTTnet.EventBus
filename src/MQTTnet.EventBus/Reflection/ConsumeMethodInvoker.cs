using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Reflection
{
    public class ConsumeMethodInvoker : IConsumeMethodInvoker
    {
        //public Task InvokeAsync<TConsumer, TEvent>(TConsumer consumer, params object[] args)
        //    => InvokeAsync(consumer, typeof(TEvent), args);

        //public Task InvokeAsync<TConsumer>(TConsumer consumer, Type eventType, params object[] args)
        //{
        //    var contextType = typeof(EventContext<>).MakeGenericType(eventType);
        //    var context = Activator.CreateInstance(contextType, args);

        //    return (Task)consumer.GetType()
        //        .GetMethod(nameof(IConsmer<object>.ConsumeAsync))
        //        .Invoke(consumer, new object[] { context });
        //}

        public Task InvokeAsync(object consumer, Type eventType, object[] args)
        {
            var contextType = typeof(EventContext<>).MakeGenericType(eventType);
            var context = Activator.CreateInstance(contextType, args);

            return (Task)consumer.GetType()
                .GetMethod(nameof(IConsumer<object>.ConsumeAsync))
                .Invoke(consumer, new object[] { context });
        }
    }
}
