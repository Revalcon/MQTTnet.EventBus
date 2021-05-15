namespace MQTTnet.EventBus
{
    public class EventContext
    {
        private readonly MqttApplicationMessageReceivedEventArgs message;

        public EventContext(MqttApplicationMessageReceivedEventArgs message)
        {
            this.message = message;
        }

        public string ClientId => message.ClientId;
        public bool ProcessingFailed => message.ProcessingFailed;
        public MqttApplicationMessage Message => message.ApplicationMessage;
    }

    public class EventContext<TEvent> : EventContext
    {
        public EventContext(TEvent eventArg, MqttApplicationMessageReceivedEventArgs message)
            : base(message)
        {
            EventArg = eventArg;
        }

        public TEvent EventArg { get; }
    }
}
