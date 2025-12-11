using System.IO;
using Newtonsoft.Json;

namespace MqttBrokerBlazor
{
    public class HostConfig
    {
        public static string Filename = $"{nameof(HostConfig)}.json";

        public bool HideConfigPanel = false;

        public int TcpPort = 1883;

        public int HttpPort = 5000;

        // 添加是否启用身份验证的选项
        public bool EnableAuthentication = false;
        
        // 添加用户名和密码配置
        public string Username = "admin";
        public string Password = "password";

        public static HostConfig LoadFromFile()
        {
            if (File.Exists(Filename))
            {
                using var file = File.OpenText(Filename);
                var result = (HostConfig?)new JsonSerializer().Deserialize(file, typeof(HostConfig));
                return result ?? new HostConfig();
            }
            return new HostConfig();
        }

        public void SaveToFile()
        {
            using var file = File.CreateText(Filename);
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, this);
        }
    }
}