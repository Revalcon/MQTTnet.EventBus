namespace MQTTnet.EventBus.Serializers
{
    public interface IEventDeserializer<out T>
    {
        T Deserialize(byte[] value);
    }
}
