using System;
using Microsoft.AspNetCore.Components;
using MqttBrokerBlazor.MqttBroker;
using MQTTnet.Server;

namespace MqttBrokerBlazor.Components.Panels
{
    public partial class ClientsPanel : ComponentBase, IDisposable
    {
        [Inject] MqttBrokerService _mqtt { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
        }

        // 更新事件参数 type
        void OnClientConnected(ClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        // 更新事件参数 type
        void OnClientDisconnected(ClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);
    }
}