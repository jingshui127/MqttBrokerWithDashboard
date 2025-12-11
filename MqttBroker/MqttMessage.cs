using System;
using MQTTnet;

namespace MqttBrokerBlazor.MqttBroker
{
    public class MqttMessage
    {
        public DateTime Timestamp { get; set; }
        public MqttClient? Client { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public MqttApplicationMessage Original { get; set; } = new MqttApplicationMessage();
    }
}