using HardwareAgent.Domain.Options;
using HardwareAgent.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

#region Options

builder.Services.Configure<LanguageModelOptions>(builder.Configuration.GetSection("LanguageModelOptions"));
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetSection("DiscordOptions"));

var languageModelOptions = builder.Configuration.GetSection("LanguageModelOptions").Get<LanguageModelOptions>()!;

#endregion

#region Service

builder.Services.AddSingleton<DiscordService>();
builder.Services.AddSingleton<OrchestratorService>();
builder.Services.AddSingleton<LanguageModelService>();
builder.Services.AddSingleton<DeviceService>();

#endregion

#region HostedService

builder.Services.AddHostedService(sp => sp.GetRequiredService<DiscordService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<OrchestratorService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<DeviceService>());

#endregion

#region HttpClient

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("AnythingLLM", options =>
{
    options.Timeout = TimeSpan.FromMinutes(3);
    options.BaseAddress = new Uri(languageModelOptions.BaseUrl);
    options.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", languageModelOptions.ApiKey);
});

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();