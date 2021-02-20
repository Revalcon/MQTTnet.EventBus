using MQTTnet.Client;
using System;

namespace MQTTnet.EventBus
{
    public interface IMqttPersisterConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IMqttClient GetClient();
    }
}