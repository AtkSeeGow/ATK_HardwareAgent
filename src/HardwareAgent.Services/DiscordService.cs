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
        private readonly DiscordServiceOptions discordServiceOptions;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly OrchestratorService orchestratorService;

        public DiscordService(
            ILogger<DiscordService> logger,
            IOptions<DiscordServiceOptions> discordServiceOptions,
            OrchestratorService orchestratorService)
        {
            this.logger = logger;
            this.discordServiceOptions = discordServiceOptions.Value;
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

            await discordSocketClient.LoginAsync(TokenType.Bot, this.discordServiceOptions.Token);
            await discordSocketClient.StartAsync();

            await Task.Delay(-1, stoppingToken);
        }

        private Task Log(LogMessage logMessage)
        {
            logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage.Author.Id == discordSocketClient.CurrentUser.Id)
                return;

            await this.orchestratorService.DiscordTasks.Writer.WriteAsync(new DiscordTask() { SocketMessage = socketMessage });
        }
    }
}
