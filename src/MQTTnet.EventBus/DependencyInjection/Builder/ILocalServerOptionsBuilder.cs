namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface ILocalServerOptionsBuilder : IBuilder<BusOptions>
    {
        ILocalServerOptionsBuilder RetryCount(int value);
        ILocalServerOptionsBuilder MaxConcurrentCalls(byte value);
    }
}
