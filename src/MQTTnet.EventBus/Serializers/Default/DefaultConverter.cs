using System.Text;

namespace MQTTnet.EventBus.Serializers.Default
{
    public interface IDefaultConverter : IEventConverter<string> { }

    public class DefaultConverter : IDefaultConverter
    {
        public virtual string Deserialize(byte[] value) => Encoding.UTF8.GetString(value);

        public virtual byte[] Serialize(string value) => Encoding.UTF8.GetBytes(value);
    }
}
