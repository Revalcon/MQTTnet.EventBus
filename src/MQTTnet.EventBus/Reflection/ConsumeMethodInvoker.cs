using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Reflection
{
    public class ConsumeMethodInvoker : IConsumeMethodInvoker
    {
        private readonly IEventProvider _eventProvider;

        public ConsumeMethodInvoker(IEventProvider eventProvider)
        {
            _eventProvider = eventProvider;
        }

        public Task InvokeAsync(IServiceProvider serviceProvider, SubscriptionInfo subscriptionInfo, MqttApplicationMessageReceivedEventArgs messageReceived)
        {
            var converterType = _eventProvider.GetConverterType(subscriptionInfo.EventName);
            var converter = ((Serializers.IEventDeserializer<object>)serviceProvider.GetService(converterType));

            var message = messageReceived.ApplicationMessage;
            var eventArg = converter.Deserialize(message.Payload);
            _eventProvider.SetTopicInfo(subscriptionInfo.EventName, eventArg, message.Topic);

            var contextType = typeof(EventContext<>).MakeGenericType(subscriptionInfo.EventType);
            var context = Activator.CreateInstance(contextType, new object[] { eventArg, messageReceived });

            var consumer = serviceProvider.GetService(subscriptionInfo.ConsumerType);
            return (Task)consumer.GetType()
                .GetMethod(nameof(IConsumer<object>.ConsumeAsync))
                .Invoke(consumer, new object[] { context });
        }
    }
}
