using Microsoft.Extensions.DependencyInjection;
using MQTTnet.EventBus.Serializers;
using MQTTnet.EventBus.Serializers.String;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.ConfigurationApp
{
    class Program
    {
        private static IServiceProvider _provider;

        static async Task Main(string[] args)
        {
            Configure();

            var eventBus = _provider.GetService<IEventBus>();
            await eventBus.SubscribeAsync<MyEvent>("/state/#");

            Console.ReadLine();
        }

        public static void Configure()
        {
            var services = new ServiceCollection();

            services.AddMqttEventBus((host, service) =>
            {
                host
                    .WithClientId($"Artyom Test {Guid.NewGuid()}")
                    .WithTcpServer("IP Address", port: 1883);

                service.AddEvenets(eventBuilder =>
                {
                    eventBuilder.AddConsumer<MyEvent, MyConsumer>(cfg =>
                    {
                        cfg.UseConverter<MyConverter>();
                        cfg.UseMessageBuilder(builder => builder.WithRetainFlag());
                    });
                });

                service.AddLogger(provider => provider.UseSerilog(cfg =>
                {
                    cfg.WriteTo.Console(
                        outputTemplate: "{Timestamp:HH:mm} [{Category} {Level}] ({ThreadId}) {Message}{NewLine}{Exception}");
                }));
            });

            _provider = services.BuildServiceProvider();
        }
    }

    public class MyConverter : IEventConverter<MyEvent>
    {
        private IStringConverter _stringConverter;

        public MyConverter(IStringConverter stringConverter)
        {
            _stringConverter = stringConverter;
        }

        public MyEvent Deserialize(byte[] value)
        {
            string data = _stringConverter.Deserialize(value);
            if (!int.TryParse(data, out int status))
                status = -1;

            return new MyEvent { Status = status };
        }

        public byte[] Serialize(MyEvent value)
        {
            string data = string.Format($"{value.Status}{value.Status}");
            return _stringConverter.Serialize(data);
        }
    }

    public class MyEvent
    {
        public int Status { get; set; }
    }

    public class MyConsumer : IConsumer<MyEvent>
    {
        public Task ConsumeAsync(EventContext<MyEvent> context)
        {
            try
            {
                var status = context.EventArg.Status;
                Console.WriteLine($"{context.Message.Topic} Status: {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}
