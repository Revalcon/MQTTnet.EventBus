using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IIntegrationEventHandler<in T>
        where T : MqttApplicationMessageReceivedEventArgs
    {
        Task Handle(T args);
    }

    public interface IIntegrationEventHandler : IIntegrationEventHandler<MqttApplicationMessageReceivedEventArgs>
    { }
}