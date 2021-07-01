using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Client.Options;
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
            //var managedOptions = new ManagedMqttClientOptionsBuilder()
            //    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            //    .WithClientOptions(options)
            //    .Build();

            Configure();

            var eventBus = _provider.GetService<IEventBus>();
            //await eventBus.PublishAsync(new StatusChanged { Value = 1 }, new StatusChangedTopicInfo { Territory = "garni" });
            //await eventBus.PublishAsync(new StateChanged { Value = 1 }, new StateChangedTopicInfo { Territory = "garni" });

            //await eventBus.PublishAsync(new MyEvent { Status = 1 });
            //await eventBus.SubscribeAsync<MyEvent>("/status/garni/sever/1");
            //await eventBus.SubscribeAsync<AllEvents>("#");

            //await eventBus.SubscribeAsync<StateChanged>("/state/Ohanavan/server1/raspberry_client");
            //await eventBus.SubscribeAsync<Sys>("$SYS/broker/bytes/received");


            //await eventBus.SubscribeAsync<Sys>("$SYS/broker/subscriptions/count");
            await eventBus.SubscribeAsync<StateChanged>("/state/#");
            await eventBus.SubscribeAsync<StatusChanged>("/status/#");
            //await eventBus.SubscribeAsync<StateChanged>(new StateChangedTopicInfo { Territory = "garni", Server = "Server1" });
            //await eventBus.SubscribeAsync<StatusChanged>(new StatusChangedTopicInfo { Territory = "garni", Server = "Server1" });

            //await eventBus.PublishAsync(new StateChanged { Value = 10 });
            //var result1 = await eventBus.PublishAsync(new StateChanged { Value = 10 }, new StateChangedTopicInfo { Territory = "garni", Server = "Server1" });
            //var result2 = await eventBus.PublishAsync(new StatusChanged { Value = 10 }, new StatusChangedTopicInfo { Territory = "garni", Server = "Server1" });
            //var code = result.ReasonCode;

            ////await eventBus.PublishAsync(new StatusChanged { }, new StatusChangedTopicInfo { });
            ////await eventBus.PublishAsync(new StateChanged { }, new StateChangedTopicInfo { });

            //await eventBus.PublishAsync(new MyEvent { Territory = "garni", NodeId = "49", Status = 1 });
            //await eventBus.PublishAsync(new MyEvent { }, "/state/garni/49");

            Console.ReadLine();
        }

        public static void Configure()
        {
            var services = new ServiceCollection();

            services.AddMqttEventBus((host, localServer, service) =>
            {
                host
                    .WithClientId($"Artyom Test {Guid.NewGuid()}")
                //.WithTcpServer("5.189.161.209", port: 1883);
                    .WithTcpServer("localhost", port: 1883);
                //.WithCredentials("art", "1234");

                localServer
                    .RetryCount(5)
                    .MaxConcurrentCalls(2);

                service.AddEvenets(eventBuilder =>
                {
                    //eventBuilder.AddEventMapping<Sys>(cfg =>
                    //{
                    //    cfg.AddConsumer<SysConsumer>();
                    //    cfg.UseConverter<SysConverter>();
                    //});

                    eventBuilder.AddEventMapping<StatusChanged>(cfg =>
                    {
                        cfg.AddConsumer<StatusChangedConsumer>();
                        cfg.UseConverter<StatusChangedConverter>();
                        cfg.UseTopicPattern<StatusChangedTopicInfo>(ev => $"/status/{ev.Territory}/{ev.Server}");
                        cfg.UseMessageBuilder(mb => mb.WithAtLeastOnceQoS());
                    });

                    eventBuilder.AddEventMapping<StateChanged>(cfg =>
                    {
                        cfg.AddConsumer<StateChangedConsuer>();
                        cfg.UseConverter<StateChangedConverter>();
                        //cfg.UseJsonConverter();
                        //cfg.UseTopicPattern("/State/Test");
                        //cfg.UseTopicPattern("/State/{Territory}/{Server}/{ClientId}");
                        cfg.UseTopicPattern<StateChangedTopicInfo>(ev => $"/state/{ev.Territory}/{ev.Server}/{ev.ClientId}");
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

    public class Sys { }
    public class SysConsumer : IConsumer<Sys>
    {
        public Task ConsumeAsync(EventContext<Sys> context)
        {
            return Task.CompletedTask;
        }
    }

    public class SysConverter : IEventConverter<Sys>
    {
        public Sys Deserialize(byte[] value)
        {
            return new Sys();
        }

        public byte[] Serialize(Sys value)
        {
            return new byte[0];
        }
    }

    public class StatusChanged
    {
        public int Value { get; set; }
    }

    public interface IStatusChangedConsumer : IConsumer<StatusChanged> { }

    public class StatusChangedConsumer : IStatusChangedConsumer
    {
        public Task ConsumeAsync(EventContext<StatusChanged> context)
        {
            try
            {
                var topicInfo = context.GetTopicInfo<StatusChangedTopicInfo>();
                Console.WriteLine($"{context.Message.Topic} Status: {context.EventArg.Value}, Territory: {topicInfo.Territory}, Server: {topicInfo.Server}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }
    }

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

    public class StateChangedTopicInfo : ITopicPattern<StateChanged>
    {
        public string Territory { get; set; }
        public string Server { get; set; } = "Server1";
        public string ClientId => $"{Territory}_rpi";

        public Func<StateChangedTopicInfo, string> Create()
            => ev => $"/state/{ev.Territory}/{ev.Server}/{ev.ClientId}";
    }

    public class StatusChangedTopicInfo : ITopicPattern<StatusChanged>
    {
        public string Territory { get; set; }
        public string Server { get; set; } = "Server1";
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
            //await Task.Delay(4000);

            lock (_lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"------------");
                Console.ResetColor();
            }

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
