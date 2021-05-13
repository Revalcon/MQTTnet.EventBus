using MQTTnet.EventBus.Serializers;
using Newtonsoft.Json;

namespace MQTTnet.EventBus.Newtonsoft.Json
{
    public class NewtonsoftJsonConverter<T> : IEventConverter<T>
    {
        public T Deserialize(byte[] value)
        {
            string data = TextConvert.ToUTF8String(value);
            return JsonConvert.DeserializeObject<T>(data);
        }

        public byte[] Serialize(T value)
        {
            var data = JsonConvert.SerializeObject(value);
            return TextConvert.ToUTF8ByteArray(data);
        }
    }
}
