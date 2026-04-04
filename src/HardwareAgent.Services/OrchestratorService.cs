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

        public readonly Channel<DataEnvelope> DataEnvelopeTasks = Channel.CreateUnbounded<DataEnvelope>();

        public OrchestratorService(
            IServiceProvider serviceProvider,
            ILogger<OrchestratorService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var dataEnvelopeTasksWorker = processDataEnvelopeTasks(stoppingToken);
            await Task.WhenAll(dataEnvelopeTasksWorker);
        }

        private async Task processDataEnvelopeTasks(CancellationToken stoppingToken)
        {
            await foreach (var dataEnvelopeTasks in DataEnvelopeTasks.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var languageModelService = scope.ServiceProvider.GetRequiredService<LanguageModelService>();
                    var discordService = scope.ServiceProvider.GetRequiredService<DiscordService>();

                    var destination = dataEnvelopeTasks.Destination;
                    if (destination == EndpointType.LanguageModel)
                    {
                        var textResponse = dataEnvelopeTasks.Payload;
                        //var textResponse = await languageModelService.AskAsync("atk_hardwareagent", dataEnvelopeTasks.Payload);
                        await this.DataEnvelopeTasks.Writer.WriteAsync(new DataEnvelope()
                        {
                            Source = EndpointType.LanguageModel,
                            Destination = EndpointType.Discord,
                            ContentType = "text/plain",
                            Payload = textResponse,
                            Metadata = dataEnvelopeTasks.Metadata
                        });
                    }
                    else if(destination == EndpointType.Discord)
                    {
                        var channelId = ulong.Parse(dataEnvelopeTasks.Metadata["ChannelId"].ToString());
                        await discordService.SendMessageAsync(dataEnvelopeTasks.Payload, channelId);
                    }
                    else if (destination == EndpointType.Device)
                    {

                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
