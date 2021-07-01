namespace MQTTnet.EventBus
{
    public class BusOptions
    {
        public int RetryCount { get; set; } = 5;
        public byte MaxConcurrentCalls { get; set; } = 10;
    }
}
