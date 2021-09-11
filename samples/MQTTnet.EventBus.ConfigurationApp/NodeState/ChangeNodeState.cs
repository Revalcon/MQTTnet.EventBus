namespace MQTTnet.EventBus.ConfigurationApp
{
    //Open, Close or Refrash NodeState
    public class ChangeNodeState
    {
        public int NodeId { get; set; }
        public int ActionType { get; set; }
    }
}
