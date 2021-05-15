using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQTTnet.EventBus.Reflection;
using MQTTnet.EventBus.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Tests
{
    [TestClass]
    public class ConsumeMethodInvokerTest
    {
        [TestMethod]
        public async Task TestConsmerMethod()
        {
            //var eventArgs = new MyEvent { Id = 1, Name = "A1" };
            //var consumer = new MyConsumer();

            //IConsumeMethodInvoker invoker = new ConsumeMethodInvoker();
            //await invoker.InvokeAsync(consumer, typeof(MyEvent), typeof(JsonDeserializer<>), null);

            //Assert.IsTrue(consumer.Cache.Count == 1);

            //var kvp = consumer.Cache.First();
            //Assert.IsTrue(kvp.Key == eventArgs.Id && kvp.Value == eventArgs.Name);
        }
    }

    public class MyEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MyConsumer : IConsumer<MyEvent>
    {
        public Dictionary<int, string> Cache { get; set; } = new Dictionary<int, string>();

        public Task ConsumeAsync(EventContext<MyEvent> context)
        {
            var arg = context.EventArg;
            Cache.Add(arg.Id, arg.Name);
            return Task.CompletedTask;
        }
    }

    class JsonDeserializer<T> : IEventDeserializer<T>
    {
        public T Deserialize(byte[] value)
        {
            //string jsonMessage = Encoding.UTF8.GetString(value);
            string jsonMessage = JsonConvert.SerializeObject(new { Id = 1, Name = "A1" });
            return JsonConvert.DeserializeObject<T>(jsonMessage);
        }
    }
}
