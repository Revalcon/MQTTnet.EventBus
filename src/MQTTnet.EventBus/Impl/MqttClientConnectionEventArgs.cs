using MQTTnet.Client.Disconnecting;
using System;

namespace MQTTnet.EventBus.Impl
{
    public class MqttClientConnectionEventArgs
    {
        public MqttClientConnectionEventArgs(string clientId)
        {
            ClientId = clientId;
        }

        public MqttClientConnectionEventArgs(string clientId, MqttClientDisconnectReason reason)
            : this(clientId)
        {
            DisconnectReason = reason;
        }

        public string ClientId { get; }
        public MqttClientDisconnectReason DisconnectReason { get; }
        public bool IsReConnected { get; internal set; }
        public DateTime DisconnectionTime { get; internal set; }
        public DateTime ConnectionTime { get; internal set; }

        internal void MarkAsConnected()
        {
            ConnectionTime = DateTime.Now;
            IsReConnected = true;
        }

        internal void MarkAsDisconnected()
        {
            DisconnectionTime = DateTime.Now;
            IsReConnected = false;
        }

        public static MqttClientConnectionEventArgs Disconnected(string clientId, MqttClientDisconnectReason reason) => 
            new MqttClientConnectionEventArgs(clientId, reason)
            {
                DisconnectionTime = DateTime.Now,
                IsReConnected = false
            };

        public static MqttClientConnectionEventArgs Connected(string clientId) => 
            new MqttClientConnectionEventArgs(clientId)
            {
                ConnectionTime = DateTime.Now,
                IsReConnected = true
            };
    }
}
