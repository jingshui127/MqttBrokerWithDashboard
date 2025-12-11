using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MqttBrokerBlazor.Components.Dialogs;
using MqttBrokerBlazor.MqttBroker;
using MudBlazor;

namespace MqttBrokerBlazor.Components.Panels
{
    public partial class ConfigurationPanel : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; } = default!;

        [Inject] ILogger<ConfigurationPanel> _log { get; set; } = default!;

        [Inject] IDialogService _dlg { get; set; } = default!;

        bool _hidePanel;

        int _tcpPort;

        int _httpPort;
        
        // 添加是否启用身份验证的选项
        bool _enableAuthentication;
        
        // 添加用户名和密码字段
        string _username = string.Empty;
        string _password = string.Empty;

        // 添加计算属性来确定身份验证是否启用
        bool IsAuthEnabled => _enableAuthentication;

        bool IsDirty =>
            _hidePanel != Program.HostConfig.HideConfigPanel ||
            _tcpPort != Program.HostConfig.TcpPort ||
            _httpPort != Program.HostConfig.HttpPort ||
            _enableAuthentication != Program.HostConfig.EnableAuthentication ||
            _username != Program.HostConfig.Username ||
            _password != Program.HostConfig.Password;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Reset();
        }

        void Reset()
        {
            _hidePanel = Program.HostConfig.HideConfigPanel;
            _tcpPort = Program.HostConfig.TcpPort;
            _httpPort = Program.HostConfig.HttpPort;
            _enableAuthentication = Program.HostConfig.EnableAuthentication;
            _username = Program.HostConfig.Username;
            _password = Program.HostConfig.Password;
        }

        void SaveToFile()
        {
            Program.HostConfig.HideConfigPanel = _hidePanel;
            Program.HostConfig.TcpPort = _tcpPort;
            Program.HostConfig.HttpPort = _httpPort;
            Program.HostConfig.EnableAuthentication = _enableAuthentication;
            Program.HostConfig.Username = _username;
            Program.HostConfig.Password = _password;

            Program.HostConfig.SaveToFile();
        }

        async Task SaveAndRestart()
        {
            var parameters = new DialogParameters();
            parameters.Add("ContentText", "您真的要保存更改并重启服务器吗？");
            parameters.Add("ButtonText", "保存并重启");
            parameters.Add("Color", Color.Error);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };

            var dialog = await _dlg.ShowAsync<ConfirmationDialog>("保存并重启？", parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                _log.LogWarning("Save config and restart server ...");
                SaveToFile();
                Program.RestartHost();
            }
        }

        async Task HidePanelChanged(bool newValue)
        {
            if (newValue)
            {
                var parameters = new DialogParameters();
                parameters.Add("ContentText", $"您真的要在启动时隐藏配置面板吗？此选项只能在保存后的'{HostConfig.Filename}'文件中撤销。");
                parameters.Add("ButtonText", "确定");
                parameters.Add("Color", Color.Error);

                var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };

                var dialog = await _dlg.ShowAsync<ConfirmationDialog>("隐藏配置面板？", parameters, options);
                var result = await dialog.Result;
                if (result.Canceled) return;
            }

            _hidePanel = newValue;
        }
        
        
        // 添加密码可见性切换功能
        bool _showPassword = false;
        void TogglePasswordVisibility()
        {
            _showPassword = !_showPassword;
        }
    }
}