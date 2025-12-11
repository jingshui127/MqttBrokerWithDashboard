using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// 更新MQTTnet相关的using语句
using MQTTnet.AspNetCore;
using MQTTnet.Server;
using NewLife;
using NewLife.Log;
using System.Threading;
using System.Threading.Tasks;
using MqttBrokerBlazor.MqttBroker;
using MudBlazor.Services;
using Microsoft.Extensions.Logging;
using System;

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
                    
                    webBuilder.ConfigureServices((ctx, services) =>
                    {
                        services.AddRazorPages(options => options.RootDirectory = "/Pages");
                        services.AddServerSideBlazor();
                        services.AddStardust("http://47.113.219.65:6600", "MqttBrokerBlazor", null);

                        services.AddMudServices();

                        services.AddSingleton<MqttBrokerService>();
                        
                        // 更新MQTT服务配置以适应新版本
                        services.AddHostedMqttServerWithServices(options =>
                        {
                            // 移除不再存在的WithInterceptor调用
                            options.WithoutDefaultEndpoint();
                        });
                        
                        services.AddMqttConnectionHandler();
                        services.AddConnections();
                    });
                    
                    webBuilder.Configure((ctx, app) =>
                    {
                        var env = ctx.HostingEnvironment;
                        var services = app.ApplicationServices;
                        var mqttService = services.GetRequiredService<MqttBrokerService>();
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        
                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseStaticFiles();

                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapBlazorHub();
                            endpoints.MapFallbackToPage("/_Host");
                            endpoints.MapMqtt("/mqtt");
                        });

                        app.UseMqttServer(server =>
                        {
                            mqttService.Server = server;

                            // 更新事件处理器注册方式
                            server.ClientConnectedAsync += mqttService.HandleClientConnectedAsync;
                            server.ClientDisconnectedAsync += mqttService.HandleClientDisconnectedAsync;
                            server.InterceptingPublishAsync += mqttService.HandleApplicationMessageReceivedAsync;
                            
                            // 添加身份验证处理
                            server.ValidatingConnectionAsync += (e) => ValidateConnectionAsync(e, logger);
                        });
                    });
                });

        public static void RestartHost() =>
            _manualResartCts.Cancel();
            
        // 添加连接验证方法
        private static Task ValidateConnectionAsync(ValidatingConnectionEventArgs e, ILogger logger)
        {
            // 记录连接尝试的日志
            logger.LogInformation($"Connection attempt from client {e.ClientId}, UserName: '{e.UserName}', Password: '{e.Password}'");
            
            // 检查客户端ID是否存在
            if (string.IsNullOrEmpty(e.ClientId))
            {
                logger.LogInformation("ClientId is null or empty, rejecting connection");
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ClientIdentifierNotValid;
                return Task.CompletedTask;
            }
            
            // 获取HostConfig实例
            var hostConfig = HostConfig.LoadFromFile();
            logger.LogInformation($"HostConfig - EnableAuthentication: {hostConfig.EnableAuthentication}, Username: '{hostConfig.Username}', Password: '{hostConfig.Password}'");
            
            // 如果未启用身份验证，则允许连接，不需要检查用户名和密码
            if (!hostConfig.EnableAuthentication)
            {
                logger.LogInformation("Authentication disabled, allowing connection");
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                return Task.CompletedTask;
            }
            
            // 检查用户名是否存在
            if (string.IsNullOrEmpty(e.UserName))
            {
                logger.LogInformation("UserName is null or empty, rejecting connection");
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                return Task.CompletedTask;
            }
            
            // 检查密码是否存在
            if (string.IsNullOrEmpty(e.Password))
            {
                logger.LogInformation("Password is null or empty, rejecting connection");
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                return Task.CompletedTask;
            }
            
            // 检查用户名和密码是否匹配
            if (e.UserName != hostConfig.Username || e.Password != hostConfig.Password)
            {
                logger.LogInformation("UserName or Password mismatch, rejecting connection");
                e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                return Task.CompletedTask;
            }
            
            logger.LogInformation("Authentication successful, allowing connection");
            e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            return Task.CompletedTask;
        }
    }
}