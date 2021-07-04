using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQTTnet.EventBus
{
    public static class AsyncEnumerableExtansions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (var item in source)
                list.Add(item);
            return list;
        }
    }
}
