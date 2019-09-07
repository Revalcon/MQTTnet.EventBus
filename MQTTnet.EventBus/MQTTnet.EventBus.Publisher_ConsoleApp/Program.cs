using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Publisher_ConsoleApp
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var mqttClient = new MqttFactory().CreateMqttClient();

            //var options = new MqttClientOptionsBuilder()
            //    .WithTcpServer(server, 1883) // Port is optional
            //    .Build();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Console_App_Publisher")
                .WithTcpServer("{Ip Address}", 1883)
                //.WithTcpServer("m11.cloudmqtt.com", 14177)
                //.WithCredentials("node1", "123456")
                .Build();

            mqttClient.UseDisconnectedHandler(async e =>
            {
                await mqttClient.ConnectAsync(options);
            });

            await mqttClient.ConnectAsync(options);

            Console.WriteLine("MyTopic1");
            while (true)
            {
                Console.Write("message: ");
                string messageText = Console.ReadLine();

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("MyTopic1")
                    .WithPayload(messageText)
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

                try
                {
                    await mqttClient.PublishAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
