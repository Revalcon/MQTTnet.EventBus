using Microsoft.Extensions.DependencyInjection;
using MQTTnet.EventBus.Serializers;
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

            //await eventBus.PublishAsync(new MyEvent { Status = 1 });
            //await eventBus.SubscribeAsync<MyEvent>("/status/garni/sever/1");
            //await eventBus.SubscribeAsync<AllEvents>("#");

            //await eventBus.SubscribeAsync<StateChanged>("/state/Ohanavan/server1/raspberry_client");
            await eventBus.SubscribeAsync<StateChanged>("/state/#");


            //await eventBus.PublishAsync(new MyEvent { Territory = "garni", NodeId = "49", Status = 1 });
            //await eventBus.PublishAsync(new MyEvent { }, "/state/garni/49");

            Console.ReadLine();
        }

        public static void Configure()
        {
            var services = new ServiceCollection();

            services.AddMqttEventBus((host, service) =>
            {
                host
                    .WithClientId($"localhost-{Guid.NewGuid()}")
                    .WithTcpServer("localhost", port: 1883);

                service.AddEvenets(eventBuilder =>
                {
                    //eventBuilder.AddEventMapping<StatusChanged>(cfg =>
                    //{
                    //    cfg.AddConsumer<IStatusChangedConsumer>();
                    //    cfg.UseConverter<StatusChangedConverter>();
                    //    cfg.UseTopicPattern(ev => $"Status/{ev.Territory}/{ev.Server}/{ev.TrackerId}");
                    //    cfg.UseMessageBuilder(mb => mb.WithAtLeastOnceQoS());
                    //});

                    eventBuilder.AddEventMapping<StateChanged>(cfg =>
                    {
                        cfg.AddConsumer<StateChangedConsuer>();
                        cfg.UseConverter<StateChangedConverter>();
                        //cfg.UseJsonConverter();
                        //cfg.UseTopicPattern<StateChangedTopicInfo>(ev => $"/State/{ev.Territory}/{ev.Server}/{ev.ClientId}");
                        cfg.UseTopicPattern<StateChangedTopicInfo>(ev => $"/State/{ev.Territory}/{ev.Server}/{ev.ClientId}");
                        cfg.UseMessageBuilder(builder => builder.WithRetainFlag());
                    });

                    //eventBuilder.AddEventMapping<MyEvent>(cfg =>
                    //{
                    //    cfg.AddConsumer<IMyConsumer>();
                    //    cfg.UseConverter<MyConverter>();
                    //    cfg.UseTopicPattern(ev => $"/State/{ev.Territory}/{ev.NodeId}");
                    //    cfg.UseMessageBuilder(builder => builder.WithRetainFlag());
                    //});

                    //eventBuilder.AddEventMapping<AllEvents>(cfg =>
                    //{
                    //    cfg.AddConsumer<AllEventsConsumer>();
                    //    cfg.UseConverter<AllEventConverter>();
                    //    cfg.UseMessageBuilder(builder => builder.WithRetainFlag());
                    //});
                });

                service.AddLogger(provider => provider.UseSerilog(cfg =>
                {
                    cfg.WriteTo.Console(
                        outputTemplate: "{Timestamp:HH:mm} [{Category} {Level}] ({ThreadId}) {Message}{NewLine}{Exception}");
                }));
            });

            services.AddScoped<IMyConsumer, MyConsumer>();
            _provider = services.BuildServiceProvider();
        }
    }

    public class StatusChanged
    {
        public string Territory { get; set; }
        public string Server { get; set; } = "Server1";
        public int TrackerId { get; set; }
        public int Value { get; set; }
    }

    public interface IStatusChangedConsumer : IConsumer<StatusChanged> { }

    public class StatusChangedConverter : IEventConverter<StatusChanged>
    {
        public StatusChanged Deserialize(byte[] body)
        {
            var actin = TextConvert.ToUTF8String(body);
            if (!int.TryParse(actin, out int value))
                value = -1;

            return new StatusChanged { Value = value };
        }

        public byte[] Serialize(StatusChanged @event)
            => TextConvert.ToUTF8ByteArray(Convert.ToString(@event.Value));
    }

    #region MyRegion

    public class MyConverter : IEventConverter<MyEvent>
    {
        public MyEvent Deserialize(byte[] value)
        {
            string data = TextConvert.ToUTF8String(value);
            if (!int.TryParse(data, out int status))
                status = -1;

            return new MyEvent { Status = status };
        }

        public byte[] Serialize(MyEvent value)
        {
            string data = string.Format($"{value.Status}{value.Status}");
            return TextConvert.ToUTF8ByteArray(data);
        }
    }

    public class AllEvents { }
    public class AllEventConverter : IEventConverter<AllEvents>
    {
        public AllEvents Deserialize(byte[] value)
        {
            return new AllEvents();
        }

        public byte[] Serialize(AllEvents value)
        {
            return new byte[0];
        }
    }

    public class AllEventsConsumer : IConsumer<AllEvents>
    {
        private static object _locker = new object();
        private static string topics = "";
        private static int index = 1;
        public Task ConsumeAsync(EventContext<AllEvents> context)
        {
            lock (_locker)
            {
                Console.WriteLine($"{index}: {context.Message.Topic}");
                topics += context.Message.Topic + Environment.NewLine;
                //System.IO.File.WriteAllText("topics.txt", topics);
                index++;
            }
            return Task.CompletedTask;
        }
    }

    public class MyEvent
    {
        public string NodeId { get; set; }
        public string Territory { get; set; }
        public int Status { get; set; }
    }

    public interface IMyConsumer : IConsumer<MyEvent> { }

    public class MyConsumer : IMyConsumer
    {
        public Task ConsumeAsync(EventContext<MyEvent> context)
        {
            try
            {
                var status = context.EventArg.Status;
                Console.WriteLine($"{context.Message.Topic} Status: {status}, Territory: {context.EventArg.Territory}, NodeId: {context.EventArg.NodeId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }
    }

    public class StateChangedTopicInfo
    {
        public string Territory { get; set; }
        public string Server { get; set; } = "Server1";
        public string ClientId => $"{Territory}_rpi";
    }

    public class StateChanged
    {
        //public string Territory { get; set; }
        //public string Server { get; set; } = "Server1";
        //public string ClientId => $"{Territory}_rpi";
        public int Value { get; set; }
    }

    public class StateChangedConsuer : IConsumer<StateChanged>
    {
        private static readonly object _lockObject = new object();
        public Task ConsumeAsync(EventContext<StateChanged> context)
        {
            lock (_lockObject)
            {
                var info = context.GetTopicInfo<StateChangedTopicInfo>();
                string aaa = context.GetTopicEntity("Territory");
                if (info == null)
                {
                    Console.WriteLine($"{context.Message.Topic} State: {context.EventArg.Value}");
                }
                else
                {
                    //Console.WriteLine($"{context.Message.Topic} State: {context.EventArg.Value}, Territory: {context.EventArg.Territory}, ClientId: {context.EventArg.ClientId}");
                    Console.WriteLine($"{context.Message.Topic} State: {context.EventArg.Value}, Territory: {info.Territory}, ClientId: {info.ClientId}");
                }
                return Task.CompletedTask;
            }
        }
    }

    public class StateChangedConverter : IEventConverter<StateChanged>
    {
        public StateChanged Deserialize(byte[] body)
        {
            string data = TextConvert.ToUTF8String(body);
            if (!int.TryParse(data, out int value))
                value = -1;

            return new StateChanged { Value = value };
        }

        public byte[] Serialize(StateChanged @event)
        {
            string data = @event.Value.ToString();
            return TextConvert.ToUTF8ByteArray(data);
        }
    }

    #endregion
}
