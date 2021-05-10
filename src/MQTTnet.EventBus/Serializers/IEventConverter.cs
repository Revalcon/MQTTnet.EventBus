namespace MQTTnet.EventBus.Serializers
{
    public interface IEventConverter<T> : IEventSerializer<T>, IEventDeserializer<T> { }
}
