using Discord;
using Discord.WebSocket;
using HardwareAgent.Domain;
using HardwareAgent.Domain.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HardwareAgent.Services
{
    public class DiscordService : BackgroundService
    {
        private readonly ILogger<DiscordService> logger;
        private readonly DiscordOptions discordOptions;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly OrchestratorService orchestratorService;

        public DiscordService(
            ILogger<DiscordService> logger,
            IOptions<DiscordOptions> discordOptions,
            OrchestratorService orchestratorService)
        {
            this.logger = logger;
            this.discordOptions = discordOptions.Value;
            this.orchestratorService = orchestratorService;

            discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            discordSocketClient.Log += Log;
            discordSocketClient.MessageReceived += MessageReceived;

            await discordSocketClient.LoginAsync(TokenType.Bot, this.discordOptions.Token);
            await discordSocketClient.StartAsync();

            await Task.Delay(-1, stoppingToken);
        }

        private Task Log(LogMessage logMessage)
        {
            logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(string message, ulong channelId)
        {
            var channel = discordSocketClient.GetChannel(channelId) as IMessageChannel;
            if (channel != null)
                await channel.SendMessageAsync(message);
        }

        private async Task MessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage.Author.Id == discordSocketClient.CurrentUser.Id)
                return;

            await this.orchestratorService.DataEnvelopes.Writer.WriteAsync(new DataEnvelope()
            {
                Source = EndpointType.Discord,
                Destination = EndpointType.LanguageModel,
                ContentType = "text/plain",
                Payload = socketMessage.Content,
                Metadata = new Dictionary<string, object>()
                    {
                        { "Author", socketMessage.Author.Username },
                        { "ChannelId", socketMessage.Channel.Id },
                        { "MessageId", socketMessage.Id }
                    }
            });
        }
    }
}
