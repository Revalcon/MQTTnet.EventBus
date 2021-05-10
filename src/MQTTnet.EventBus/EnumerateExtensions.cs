using System.Collections.Generic;
using System.Linq;

namespace MQTTnet.EventBus
{
    public static class EnumerateExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
            => source == null || !source.Any();

        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
            => source == null || source.Count == 0;

        public static bool IsNullOrEmpty<T>(this T[] source)
            => source == null || source.Length == 0;
    }
}
