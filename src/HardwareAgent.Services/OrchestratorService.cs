using HardwareAgent.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;

namespace HardwareAgent.Services
{
    public class OrchestratorService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<OrchestratorService> logger;
     
        public readonly Channel<DataEnvelope<object>> DataEnvelopes = Channel.CreateUnbounded<DataEnvelope<object>>();

        public OrchestratorService(
            IServiceProvider serviceProvider,
            ILogger<OrchestratorService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var dataEnvelopesWorker = ProcessDataEnvelopes(stoppingToken);
            await Task.WhenAll(dataEnvelopesWorker);
        }

        private async Task ProcessDataEnvelopes(CancellationToken stoppingToken)
        {
            await foreach (var dataEnvelope in DataEnvelopes.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var languageModelService = scope.ServiceProvider.GetRequiredService<LanguageModelService>();
                    var discordService = scope.ServiceProvider.GetRequiredService<DiscordService>();
                    var deviceService = scope.ServiceProvider.GetRequiredService<DeviceService>();

                    var destination = dataEnvelope.Destination;
                    if (destination == EndpointType.LanguageModel)
                    {
                        var textResponse = await languageModelService.AskAsync("hardwareagent", JsonSerializer.Serialize(dataEnvelope, JsonHelper.DefaultOptions));
                        var returnEnvelope = JsonSerializer.Deserialize<DataEnvelope<object>>(textResponse, JsonHelper.DefaultOptions);
                        if (returnEnvelope != null)
                            await this.DataEnvelopes.Writer.WriteAsync(returnEnvelope);
                    }
                    else if(destination == EndpointType.Discord)
                    {
                        var channelId = ulong.Parse(dataEnvelope.Metadata["ChannelId"].ToString());
                        await discordService.SendMessageAsync(dataEnvelope.Payload.ToString(), channelId);
                    }
                    else if (destination == EndpointType.Device)
                    {
                        await deviceService.SendMessageAsync(dataEnvelope);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
