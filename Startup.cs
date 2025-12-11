using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBrokerBlazor.MqttBroker;
using MQTTnet.AspNetCore;
// 移除不再存在的命名空间引用
// using MQTTnet.AspNetCore.Extensions;
using MudBlazor.Services;

namespace MqttBrokerBlazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MqttBrokerService mqttService)
        {
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
                server.ValidatingConnectionAsync += mqttService.HandleClientConnectedAsync;
                server.ClientDisconnectedAsync += mqttService.HandleClientDisconnectedAsync;
                server.InterceptingPublishAsync += mqttService.HandleApplicationMessageReceivedAsync;
            });
        }
    }
}