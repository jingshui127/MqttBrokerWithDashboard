using Microsoft.AspNetCore.Components;
using MqttBrokerBlazor.MqttBroker;

namespace MqttBrokerBlazor.Components.Panels
{
    public partial class PublishPanel : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        string _topic = "MyTopic";

        string _payload = "MyPayload";

        bool _retained;

        bool IsPublishDisabled => string.IsNullOrWhiteSpace(_topic) || string.IsNullOrWhiteSpace(_payload);

        // 更新为异步方法调用
        async void Publish() => await _mqtt.PublishAsync(_topic, _payload, _retained);
    }
}