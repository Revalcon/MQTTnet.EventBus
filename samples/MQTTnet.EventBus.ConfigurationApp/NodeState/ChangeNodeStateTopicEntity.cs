namespace MQTTnet.EventBus.ConfigurationApp
{
    public class ChangeNodeStateTopicEntity : ITopicPattern<ChangeNodeState> 
    {
        public string Territory { get; set; }
        public string Server { get; set; } = "Server1";
    }
}
