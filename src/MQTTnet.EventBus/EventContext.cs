namespace MQTTnet.EventBus
{
    public class EventContext
    {
        private readonly MqttApplicationMessageReceivedEventArgs _message;
        protected readonly IEventProvider _eventProvider;

        public EventContext(MqttApplicationMessageReceivedEventArgs message, IEventProvider eventProvider)
        {
            _message = message;
            _eventProvider = eventProvider;
        }

        public string ClientId => _message.ClientId;
        public bool ProcessingFailed => _message.ProcessingFailed;
        public MqttApplicationMessage Message => _message.ApplicationMessage;
    }

    public class EventContext<TEvent> : EventContext
    {
        public EventContext(TEvent eventArg, MqttApplicationMessageReceivedEventArgs message, IEventProvider eventProvider)
            : base(message, eventProvider)
        {
            EventArg = eventArg;
        }

        public TEvent EventArg { get; }

        public TTopicInfo GetTopicInfo<TTopicInfo>()
            where TTopicInfo : ITopicPattern<TEvent>, new()
        {
            var topicInfo = new TTopicInfo();
            bool created = this.GetTopicInfo((provider, eventType) => provider.TrySetTopicInfo(topicInfo, eventType, Message.Topic));
            return created ? topicInfo : default;
        }
    }
}
