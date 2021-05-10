using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public interface IConsumer<T> : IConsumer
    {
        Task ConsumeAsync(EventContext<T> context);
    }

    public interface IConsumer { }
}