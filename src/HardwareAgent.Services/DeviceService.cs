using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HardwareAgent.Services
{
    public class DeviceService : BackgroundService
    {
        private readonly ILogger<DiscordService> logger;
        private readonly OrchestratorService orchestratorService;

        private ClientWebSocket clientWebSocket;
        private readonly Uri uri = new("ws://192.168.0.142:81");

        public event Action<StateMessage> OnStateChanged;

        public DeviceService(
            ILogger<DiscordService> logger,
            OrchestratorService orchestratorService)
        {
            this.logger = logger;
            this.orchestratorService = orchestratorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    clientWebSocket = new ClientWebSocket();
                    await clientWebSocket.ConnectAsync(uri, stoppingToken);
                    await ReceiveLoop(stoppingToken);
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

            while (clientWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(json);
            }
        }

        private void HandleMessage(string json)
        {
            using var doc = JsonDocument.Parse(json);

            var type = doc.RootElement.GetProperty("type").GetString();

            if (type == "state")
            {
                var state = JsonSerializer.Deserialize<StateMessage>(json);
                OnStateChanged?.Invoke(state);
            }
        }

        public async Task SetLedAsync(bool state)
        {
            if (clientWebSocket?.State != WebSocketState.Open)
            {
                return;
            }

            var cmd = new SetLedCommand
            {
                Value = state
            };

            var json = JsonSerializer.Serialize(cmd);
            var bytes = Encoding.UTF8.GetBytes(json);

            await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public class StateMessage
    {
        public bool Led { get; set; }
    }

    public class SetLedCommand
    {
        public string Type { get; set; }
        public bool Value { get; set; }

        public SetLedCommand()
        {
            Type = "set_led";
        }
    }
}
