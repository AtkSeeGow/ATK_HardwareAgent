using HardwareAgent.Domain.Options;
using HardwareAgent.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. ���U�պA (Options Pattern)
var chatOptions = builder.Configuration.GetSection("ChatOptions");
builder.Services.Configure<ChatOptions>(chatOptions);

builder.Services.AddScoped<ChatService>();

// 2. ���U Controllers
builder.Services.AddControllers();

// 3. ���U Swagger �����A�� (�����b Build ���e�I)
// �Y�ϥu�b�}�o���ҡu�ϥΡv�A�A�ȥ�����ĳ�������U�A�Ϊ̱N�P�_�޿�]�b builder ��
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("AnythingLLM", options =>
{
    options.Timeout = TimeSpan.FromMinutes(3);
});

var app = builder.Build();

// 4. �]�w HTTP �ШD�޹D (Middleware)
if (app.Environment.IsDevelopment())
{
    // �b�}�o���ұҥ� Swagger ����
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();