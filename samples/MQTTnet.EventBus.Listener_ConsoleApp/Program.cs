using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.EventBus.Listener_ConsoleApp
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            Console.Title = "Listener";

            var mqttClient = new MqttFactory().CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
               .WithClientId("Console_App_Listener")
               .WithTcpServer("{Ip Address}", 1883)
               .Build();

            await mqttClient.ConnectAsync(options);

            Console.Write("Subscribe To Topic: ");
            string topic = Console.ReadLine();

            await mqttClient.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce);

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });

            Console.ReadLine();
        }
    }
}
