using System;

namespace MqttBrokerBlazor.MqttBroker
{
    public class MqttClient
    {
        public DateTime TimeOfConnection { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public bool AllowSend { get; set; }
        public bool AllowReceive { get; set; }
    }
}