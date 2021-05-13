namespace MQTTnet.EventBus.Serializers.Text
{
    public interface IStringConverter : IEventConverter<string> { }

    public class StringConverter : IStringConverter
    {
        public virtual string Deserialize(byte[] value) => TextConvert.ToUTF8String(value);

        public virtual byte[] Serialize(string value) => TextConvert.ToUTF8ByteArray(value);
    }
}
