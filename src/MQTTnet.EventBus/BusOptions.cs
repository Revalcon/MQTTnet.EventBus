namespace MQTTnet.EventBus
{
    public class BusOptions
    {
        public int RetryCount { get; set; }
        public byte MaxConcurrentCalls { get; set; }
    }
}
