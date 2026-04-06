using HardwareAgent.Domain;
using HardwareAgent.Domain.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HardwareAgent.Services
{
    public class DeviceService : BackgroundService
    {
        private readonly ILogger<DeviceService> logger;
        private readonly DeviceOptions deviceOptions;
        private readonly OrchestratorService orchestratorService;
        private readonly Uri uri;

        private ClientWebSocket clientWebSocket;
     
        public DeviceService(
            ILogger<DeviceService> logger,
            IOptions<DeviceOptions> deviceOptions,
            OrchestratorService orchestratorService)
        {
            this.logger = logger;
            this.deviceOptions = deviceOptions.Value;
            this.orchestratorService = orchestratorService;
            this.uri = new(this.deviceOptions.Uri);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    this.clientWebSocket = new ClientWebSocket();
                    await this.clientWebSocket.ConnectAsync(uri, stoppingToken);
                    await this.ReceiveLoop(stoppingToken);
                }
                catch (Exception ex)
                {
                }
                await Task.Delay(3000, stoppingToken);
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[1024];

            while (this.clientWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var webSocketReceiveResult = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count);
                this.MessageReceived(json);
            }
        }

        private void MessageReceived(string json)
        {
            var dataEnvelope = JsonSerializer.Deserialize<DataEnvelope<object>>(json, JsonHelper.DefaultOptions);
            if (dataEnvelope != null)
                this.orchestratorService.DataEnvelopes.Writer.WriteAsync(dataEnvelope);
        }

        public async Task SendMessageAsync(DataEnvelope<object> dataEnvelope)
        {
            var json = JsonSerializer.Serialize(dataEnvelope, JsonHelper.DefaultOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
