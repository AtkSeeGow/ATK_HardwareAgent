using HardwareAgent.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace HardwareAgent.Services
{
    public class OrchestratorService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<OrchestratorService> logger;

        public readonly Channel<DiscordTask> DiscordTasks = Channel.CreateUnbounded<DiscordTask>();
        public readonly Channel<DeviceTask> DeviceTasks = Channel.CreateUnbounded<DeviceTask>();

        public OrchestratorService(
            IServiceProvider serviceProvider,
            ILogger<OrchestratorService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var discordTasksWorker = ProcessDiscordTasks(stoppingToken);
            var deviceTasksWorker = ProcessDeviceTasks(stoppingToken);
            await Task.WhenAll(discordTasksWorker, deviceTasksWorker);
        }

        private async Task ProcessDiscordTasks(CancellationToken stoppingToken)
        {
            await foreach (var discordTask in DiscordTasks.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var languageModelService = scope.ServiceProvider.GetRequiredService<LanguageModelService>();

                    var xx = await languageModelService.AskAsync("atk_hardwareagent", discordTask.SocketMessage.Content);

                    // 分析結果，如果要丟裝置就不管了

                    await discordTask.SocketMessage.Channel.SendMessageAsync(xx);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async Task ProcessDeviceTasks(CancellationToken stoppingToken)
        {
            await foreach (var deviceMessage in DeviceTasks.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var languageModelService = scope.ServiceProvider.GetRequiredService<LanguageModelService>();

                    // 呼叫設備
                    // ESP32 / PC / IoT
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
