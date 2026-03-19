using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HardwareAgent.Services
{
    public class DeviceService : BackgroundService
    {
        private readonly ILogger<DiscordService> logger;
        private readonly OrchestratorService orchestratorService;

        public DeviceService(
            ILogger<DiscordService> logger,
            OrchestratorService orchestratorService)
        {
            this.logger = logger;
            this.orchestratorService = orchestratorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(-1, stoppingToken);
        }
    }
}
