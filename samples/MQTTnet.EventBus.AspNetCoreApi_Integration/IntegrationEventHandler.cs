using System.Threading.Tasks;

namespace MQTTnet.EventBus.AspNetCoreApi_Integration
{
    public class IntegrationEventHandler : IConsumer
    {
        public Task Consume(MqttApplicationMessageReceivedEventArgs args)
        {
            //Some action...
            return Task.CompletedTask;
        }
    }
}
