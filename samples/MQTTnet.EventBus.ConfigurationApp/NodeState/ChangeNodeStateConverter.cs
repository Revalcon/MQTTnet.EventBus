using MQTTnet.EventBus.Serializers;

namespace MQTTnet.EventBus.ConfigurationApp
{
    public class ChangeNodeStateConverter : IEventConverter<ChangeNodeState>
    {
        public ChangeNodeState Deserialize(byte[] body)
        {
            var data = TextConvert.ToUTF8String(body);
            
            var strNodeId = data.Substring(0, data.Length - 1);
            if (!int.TryParse(strNodeId, out int nodeId))
                return null;

            var strActionType = data.Substring(data.Length - 1);
            if (!int.TryParse(strActionType, out int actionType))
                return null;

            return new ChangeNodeState { NodeId = nodeId, ActionType = actionType };
        }

        public byte[] Serialize(ChangeNodeState @event)
            => TextConvert.ToUTF8ByteArray($"{@event.NodeId}{@event.ActionType}");
    }
}
