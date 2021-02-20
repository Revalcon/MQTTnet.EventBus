using System.Threading.Tasks;

namespace MQTTnet.EventBus.AspNetCoreApi_Integration
{
    public class IntegrationEventHandler : IIntegrationEventHandler
    {
        public Task Handle(MqttApplicationMessageReceivedEventArgs args)
        {
            //Some action...
            return Task.CompletedTask;
        }
    }
}
