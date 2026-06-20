using FinanceAssistant.Application.Commands.GenerateMonthlyReport;
using FinanceAssistant.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinanceAssistant.Infrastructure.BackgroundServices;

public class MonthlyReportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<MonthlyReportBackgroundService> _logger;

    public MonthlyReportBackgroundService(
        IServiceProvider serviceProvider,
        ITelegramBotClient botClient,
        ILogger<MonthlyReportBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _botClient = botClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonthlyReportBackgroundService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await WaitUntilNextRunAsync(stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await SendScheduledReportsAsync(stoppingToken);
        }
    }

    // Aguarda até as 08:00 UTC do próximo dia
    private static async Task WaitUntilNextRunAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(8);
        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        var delay = nextRun - now;
        await Task.Delay(delay, stoppingToken);
    }

    private async Task SendScheduledReportsAsync(CancellationToken stoppingToken)
    {
        var today = DateTime.UtcNow.Day;
        var previousMonth = DateTime.UtcNow.AddMonths(-1);

        _logger.LogInformation("Verificando relatorios agendados para o dia {Day}", today);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var users = await userRepository.GetUsersWithReportDayAsync(today, stoppingToken);

        foreach (var user in users)
        {
            try
            {
                var report = await mediator.Send(
                    new GenerateMonthlyReportCommand(user.TelegramId, previousMonth.Year, previousMonth.Month),
                    stoppingToken);

                if (report is null)
                {
                    _logger.LogInformation("Sem transacoes para {UserId} em {Month}/{Year}",
                        user.Id, previousMonth.Month, previousMonth.Year);
                    continue;
                }

                using var stream = new MemoryStream(report.Content);
                var monthLabel = previousMonth.ToString("MMMM/yyyy");

                await _botClient.SendDocument(
                    chatId: new ChatId(user.TelegramId),
                    document: InputFile.FromStream(stream, report.FileName),
                    caption: $"Seu relatorio de {monthLabel} chegou!",
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Relatorio enviado para {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar relatorio para usuario {UserId}", user.Id);
            }
        }
    }
}
