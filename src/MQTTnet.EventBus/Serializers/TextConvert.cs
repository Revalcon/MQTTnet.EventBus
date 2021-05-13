using System.Text;

namespace MQTTnet.EventBus.Serializers
{
    public static class TextConvert
    {
        public static string ToUTF8String(byte[] value) => Encoding.UTF8.GetString(value);

        public static byte[] ToUTF8ByteArray(string value) => Encoding.UTF8.GetBytes(value);
    }
}
