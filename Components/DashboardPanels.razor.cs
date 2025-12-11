using Microsoft.AspNetCore.Components;
using MqttBrokerBlazor.MqttBroker;
using MQTTnet;
using MQTTnet.Server;

namespace MqttBrokerBlazor.Components
{
    public partial class DashboardPanels : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; } = default!;

        int _numberOfUnseenMessages = 0;

        bool _isMessagesPanelExpanded;
        bool IsMessagesPanelExpanded
        {
            get => _isMessagesPanelExpanded;

            set
            {
                if (value)
                    _numberOfUnseenMessages = 0;
                _isMessagesPanelExpanded = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
            _mqtt.OnMessageReceived -= OnMessageReceived;
        }

        // 更新事件参数类型
        void OnClientConnected(ClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        // 更新事件参数类型
        void OnClientDisconnected(ClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        // 更新事件参数类型
        void OnMessageReceived(InterceptingPublishEventArgs e)
        {
            if (!_isMessagesPanelExpanded)
                _numberOfUnseenMessages++;
            InvokeAsync(StateHasChanged);
        }
    }
}