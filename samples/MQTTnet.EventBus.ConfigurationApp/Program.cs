using Microsoft.Extensions.DependencyInjection;
using MQTTnet.EventBus.Serializers;
using MQTTnet.EventBus.Serializers.Default;
using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.ConfigurationApp
{
    class Program
    {
        private static IServiceProvider _provider;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Configure();

            var eventProvider = _provider.GetService<IEventProvider>();

            Console.ReadLine();
        }

        public static void Configure()
        {
            var services = new ServiceCollection();
            services.AddMqttEventBus((host, cfg) =>
            {
                host
                    .WithClientId("Api")
                    .WithTcpServer("{Ip Address}", port: 1883);

                cfg.AddConsumer<MyEvent, MyConsumer>(c =>
                {
                    c.UseConverter<MyConverter>();
                    c.UseMessageBuilder(builder => builder.WithRetainFlag());
                });
            });

            _provider = services.BuildServiceProvider();
        }
    }

    public class MyConverter : IEventConverter<MyEvent>
    {
        private IDefaultConverter _defaultConverter;

        public MyConverter()
        {
            _defaultConverter = new DefaultConverter();
        }

        public MyEvent Deserialize(byte[] value)
        {
            string data = _defaultConverter.Deserialize(value);
            return new MyEvent 
            {
                Number = int.Parse(data.Substring(0, 2)),
                Value = int.Parse(data.Substring(2)),
            };
        }

        public byte[] Serialize(MyEvent value)
        {
            string data = string.Format($"{value.Number}{value.Value}");
            return _defaultConverter.Serialize(data);
        }
    }

    public class MyEvent
    {
        public int Number { get; set; }
        public int Value { get; set; }
    }

    public class MyConsumer : IConsumer<MyEvent>
    {
        public Task ConsumeAsync(EventContext<MyEvent> context)
        {
            return Task.CompletedTask;
        }
    }
}
