namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IBuilder<out T>
    {
        T Build();
    }
}
