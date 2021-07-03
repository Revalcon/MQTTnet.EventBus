namespace MQTTnet.EventBus.DependencyInjection.Builder.Impl
{
    public class LocalServerOptionsBuilder : ILocalServerOptionsBuilder
    {
        private readonly BusOptions _busOptions;

        public LocalServerOptionsBuilder() : this(new BusOptions()) { }

        public LocalServerOptionsBuilder(BusOptions busOptions)
        {
            _busOptions = busOptions;
        }

        public ILocalServerOptionsBuilder MaxConcurrentCalls(byte value)
        {
            _busOptions.MaxConcurrentCalls = value;
            return this;
        }

        public ILocalServerOptionsBuilder RetryCount(int value)
        {
            _busOptions.RetryCount = value;
            return this;
        }

        public BusOptions Build() => _busOptions;
    }
}
