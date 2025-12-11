using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
// 更新MQTTnet相关的using语句
using MQTTnet.AspNetCore;
using NewLife;
using NewLife.Log;
using System.Threading;
using System.Threading.Tasks;

namespace MqttBrokerBlazor
{
    public class Program
    {
        public static HostConfig HostConfig { get; set; }

        static IHost _host;

        static CancellationTokenSource _manualResartCts;
        //public static ITracer? _tracer;


        public static async Task Main(string[] args)
        {
            //var star = new Stardust.StarFactory(null, null, null);
         
            //// star.Dump();
            //if (star.Server.IsNullOrEmpty())
            //{
            //    star.Server = "http://47.113.219.65:6600";
            //    _tracer = star.Tracer;
            //}

            HostConfig = HostConfig.LoadFromFile();

        RestartHost:
            _host = CreateHostBuilder(args).Build();

            _manualResartCts = new CancellationTokenSource();
            await _host.RunAsync(_manualResartCts.Token);

            if (_manualResartCts.IsCancellationRequested)
            {
                System.Console.WriteLine("Restarting host ...");

                _host.Dispose();
                _host = null;

                goto RestartHost;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        // 确保正确的端口配置
                        options.ListenAnyIP(HostConfig.TcpPort, l => l.UseMqtt()); // MQTT端口使用MQTT协议
                        options.ListenAnyIP(HostConfig.HttpPort); // HTTP端口
                    });
                    webBuilder.UseStartup<Startup>();
                });

        public static void RestartHost() =>
            _manualResartCts.Cancel();
    }
}