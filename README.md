# MQTT Broker 与仪表盘

一个简单的服务器端应用程序，托管MQTT Broker和仪表盘UI，使用[ASP.NET Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)进行实时监控，可快速构建和测试自定义MQTT基础设施。

![MqttBrokerBlazor](MqttBrokerBlazor.png)

## 命令行运行

1. 安装 [Microsoft .NET SDK 10.0](https://dotnet.microsoft.com/download)

2. 从GitHub克隆项目

3. 从命令行启动主机（在项目根文件夹中）

   `$ dotnet run`

4. 在浏览器中访问仪表盘UI: http://localhost:5000

## 在Docker容器中运行

1. 安装 [Docker Desktop](https://docs.docker.com/desktop)

2. 从GitHub克隆项目

3. 作为Docker服务运行（在项目根文件夹中）:

   `$ docker-compose up -d`

4. 在浏览器中访问仪表盘UI: http://localhost:5000

## 配置

端口配置存储在"HostConfig.json"中，并在启动时加载。

- TCP端口: 1883（常规_MQTT over TCP，可修改）
- HTTP端口: 5000
   - "/"显示仪表盘UI
   - "/mqtt"端点提供_MQTT over WebSocket_连接

## 依赖项

- [MQTTnet](https://github.com/chkr1011/MQTTnet) 支持MQTT over WebSockets的MQTT库
- [MudBlazor](https://mudblazor.com) 仪表盘Web前端的Material Design UI框架
- [Json.NET](https://www.newtonsoft.com/json) 用于加载/保存配置文件的Json库

## 故障排除

### 端口冲突问题

如果遇到"address already in use"错误，请检查以下几点：

1. 确保没有其他MQTT服务在运行
2. 修改HostConfig.json中的端口配置以避免冲突
3. 使用任务管理器或命令行工具终止占用端口的进程

### MQTTnet版本兼容性

本项目已更新至MQTTnet 5.0.1.1416版本，主要变更包括：
- 更新了事件处理程序API
- 修正了消息拦截器注册方式
- 适配了新的命名空间结构

## 项目结构

- `Components/`：Blazor组件
  - `Panels/`：主界面面板（客户端、消息、发布、配置）
  - `Dialogs/`：对话框组件（如确认弹窗）
  - `DashboardPanels.razor`：仪表盘主视图
- `MqttBroker/`：MQTT核心逻辑
  - `MqttBrokerService.cs`：Broker服务
  - `MqttClient.cs`：客户端管理
  - `MqttMessage.cs`：消息模型
- 配置文件：`HostConfig.json`, `appsettings.json`
- 容器配置：`Dockerfile`, `docker-compose.yml`