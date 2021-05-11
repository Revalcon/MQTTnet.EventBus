using System.Text;

namespace MQTTnet.EventBus.Serializers.String
{
    public interface IStringConverter : IEventConverter<string> { }

    public class StringConverter : IStringConverter
    {
        public virtual string Deserialize(byte[] value) => Encoding.UTF8.GetString(value);

        public virtual byte[] Serialize(string value) => Encoding.UTF8.GetBytes(value);
    }
}
