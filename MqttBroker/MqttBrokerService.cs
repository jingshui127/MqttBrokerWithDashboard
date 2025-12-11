using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MqttBrokerBlazor.MqttBroker
{
    public class MqttBrokerService
    {
        readonly ILogger _log;

        public MqttServer Server { get; set; }

        readonly object _thisLock = new();

        List<MqttMessage> _messages = new();
        public IReadOnlyList<MqttMessage> Messages
        {
            get
            {
                lock (_thisLock)
                {
                    return _messages?.AsReadOnly();
                }
            }
        }

        Dictionary<string, List<MqttMessage>> _messagesByTopic = new();
        public IReadOnlyDictionary<string, List<MqttMessage>> MessagesByTopic
        {
            get
            {
                lock (_thisLock)
                {
                    return _messagesByTopic as IReadOnlyDictionary<string, List<MqttMessage>>;
                }
            }
        }

        List<MqttClient> _connectedClients = new();
        public IReadOnlyList<MqttClient> ConnectedClients
        {
            get
            {
                lock (_thisLock)
                {
                    return _connectedClients?.AsReadOnly();
                }
            }
        }


        // 更新事件参数类型
        public event Action<ValidatingConnectionEventArgs> OnClientConnected;
        public event Action<ClientDisconnectedEventArgs> OnClientDisconnected;
        public event Action<InterceptingPublishEventArgs> OnMessageReceived;


        public MqttBrokerService(ILogger<MqttBrokerService> log) =>
            _log = log;


        // 创建新的处理方法，替代原来的接口实现
        public Task HandleClientConnectedAsync(ValidatingConnectionEventArgs e)
        {
            lock (_thisLock) _connectedClients.Add(new MqttClient
            {
                TimeOfConnection = DateTime.Now,
                ClientId = e.ClientId,
                AllowSend = true,
                AllowReceive = true,
            });

            _log.LogInformation($"Client connected: {e.ClientId}");

            OnClientConnected?.Invoke(e);
            return Task.CompletedTask;
        }

        public Task HandleClientDisconnectedAsync(ClientDisconnectedEventArgs e)
        {
            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                if (client == null)
                {
                    _log.LogError($"Unkownd client disconnected: {e.ClientId}");
                    return Task.CompletedTask;
                }

                _connectedClients.Remove(client);
            }

            _log.LogInformation($"Client disconnected: {e.ClientId}");

            OnClientDisconnected?.Invoke(e);
            return Task.CompletedTask;
        }

        public Task HandleApplicationMessageReceivedAsync(InterceptingPublishEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            // 修复Payload访问方式
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                var message = new MqttMessage
                {
                    Timestamp = DateTime.Now,
                    Client = client,
                    Topic = topic,
                    Payload = payload,
                    Original = e.ApplicationMessage,
                };

                _messages.Insert(0, message);

                if (_messagesByTopic.ContainsKey(topic))
                    _messagesByTopic[topic].Insert(0,  message);
                else
                    _messagesByTopic[topic] = new List<MqttMessage> { message };
            }

            _log.LogInformation($"OnMessageReceived: {topic} {payload}");

            OnMessageReceived?.Invoke(e);
            return Task.CompletedTask;
        }

        public Task InterceptClientMessageQueueEnqueueAsync(InterceptingClientApplicationMessageEnqueueEventArgs context)
        {
            // see https://github.com/chkr1011/MQTTnet/issues/1167
            /*
            if (!string.IsNullOrEmpty(context.SenderClientId))
            {
                var sender = _connectedClients.Find(x => x.ClientId == context.SenderClientId);
                if (sender != null && !sender.AllowSend)
                {
                    context.AcceptEnqueue = false;
                    return Task.CompletedTask;
                }
            }

            if (!string.IsNullOrEmpty(context.ReceiverClientId))
            {
                var receiver = _connectedClients.Find(x => x.ClientId == context.ReceiverClientId);
                if (receiver != null && !receiver.AllowReceive)
                {
                    context.AcceptEnqueue = false;
                    return Task.CompletedTask;
                }
            }
            */
            return Task.CompletedTask;
        }


        // 修复Publish方法
        public async Task PublishAsync(MqttApplicationMessage message)
        {
            if (Server != null)
            {
                await Server.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message));
            }
        }

        public async Task PublishAsync(string topic, byte[] payload, bool retain)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .Build();
            await PublishAsync(message);
        }

        public async Task PublishAsync(string topic, string payload, bool retain)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .Build();
            await PublishAsync(message);
        }
    }
}