using FinanceAssistant.Application;
using FinanceAssistant.Infrastructure;
using FinanceAssistant.Infrastructure.Persistence;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Inicializa banco: roda migrations pendentes e seed de categorias
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseInitializer.InitializeAsync(context);
}

// Registra webhook do Telegram se a URL estiver configurada
var webhookUrl = app.Configuration["Telegram:WebhookUrl"];
if (!string.IsNullOrWhiteSpace(webhookUrl))
{
    var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
    await botClient.SetWebhook($"{webhookUrl.TrimEnd('/')}/api/telegram");
    app.Logger.LogInformation("Webhook registrado em {Url}/api/telegram", webhookUrl);
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

app.Run();
