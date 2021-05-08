namespace MQTTnet.EventBus.Serializers
{
    public interface IEventSerializer<in T>
    {
        byte[] Serialize(T value);
    }
}
