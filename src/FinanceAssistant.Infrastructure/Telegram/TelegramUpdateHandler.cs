using System.Text;
using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.Commands.GenerateMonthlyReport;
using FinanceAssistant.Application.Commands.ProcessImage;
using FinanceAssistant.Application.Commands.ProcessMessage;
using FinanceAssistant.Application.Commands.SetReportConfig;
using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Application.Queries.GetLastTransactions;
using FinanceAssistant.Application.Queries.GetMonthlyExpenses;
using FinanceAssistant.Application.Queries.GetMonthlyIncome;
using FinanceAssistant.Application.Queries.GetMonthlySummary;
using FinanceAssistant.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinanceAssistant.Infrastructure.Telegram;

public class TelegramUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMediator _mediator;
    private readonly IAudioTranscriptionService _transcriptionService;
    private readonly ILogger<TelegramUpdateHandler> _logger;

    public TelegramUpdateHandler(
        ITelegramBotClient botClient,
        IMediator mediator,
        IAudioTranscriptionService transcriptionService,
        ILogger<TelegramUpdateHandler> logger)
    {
        _botClient = botClient;
        _mediator = mediator;
        _transcriptionService = transcriptionService;
        _logger = logger;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;

        var chatId = message.Chat.Id;
        var telegramUserId = message.From!.Id;
        var userName = message.From.FirstName ?? "Usuário";

        try
        {
            string response;

            if (message.Photo is { } photos)
            {
                _logger.LogInformation("Foto de {UserId}", telegramUserId);
                response = await HandlePhotoAsync(photos, telegramUserId, userName, cancellationToken);
            }
            else if (message.Voice is { } voice)
            {
                _logger.LogInformation("Audio de {UserId}", telegramUserId);
                response = await HandleVoiceAsync(voice, telegramUserId, userName, cancellationToken);
            }
            else if (message.Text is { } text)
            {
                _logger.LogInformation("Mensagem de {UserId}: {Text}", telegramUserId, text);

                // /relatorio envia documento — tratado separadamente
                if (text.StartsWith("/relatorio", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleReportCommandAsync(text, chatId, telegramUserId, cancellationToken);
                    return;
                }

                response = text.StartsWith('/')
                    ? await HandleCommandAsync(text, telegramUserId, cancellationToken)
                    : await HandleTextAsync(text, telegramUserId, userName, cancellationToken);
            }
            else return;

            await _botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar update {UpdateId}", update.Id);
            await _botClient.SendMessage(chatId, "Ocorreu um erro. Tente novamente.", cancellationToken: cancellationToken);
        }
    }

    private async Task<string> HandleVoiceAsync(Voice voice, long telegramUserId, string userName, CancellationToken cancellationToken)
    {
        var file = await _botClient.GetFile(voice.FileId, cancellationToken);
        using var stream = new MemoryStream();
        await _botClient.DownloadFile(file.FilePath!, stream, cancellationToken);

        var transcription = await _transcriptionService.TranscribeAsync(stream.ToArray(), cancellationToken);

        if (string.IsNullOrWhiteSpace(transcription))
            return "Nao consegui entender o audio. Tente falar mais claramente ou envie uma mensagem de texto.";

        _logger.LogInformation("Transcricao de {UserId}: {Text}", telegramUserId, transcription);
        return await HandleTextAsync(transcription, telegramUserId, userName, cancellationToken);
    }

    private async Task<string> HandlePhotoAsync(PhotoSize[] photos, long telegramUserId, string userName, CancellationToken cancellationToken)
    {
        var photo = photos.Last();
        var file = await _botClient.GetFile(photo.FileId, cancellationToken);

        using var stream = new MemoryStream();
        await _botClient.DownloadFile(file.FilePath!, stream, cancellationToken);

        return await _mediator.Send(
            new ProcessImageCommand(telegramUserId, userName, stream.ToArray()),
            cancellationToken);
    }

    private async Task<string> HandleCommandAsync(string text, long telegramUserId, CancellationToken cancellationToken)
    {
        var command = text.Split(' ')[0].ToLowerInvariant();
        var now = DateTime.UtcNow;

        return command switch
        {
            "/gastos" => FormatExpenses(
                await _mediator.Send(new GetMonthlyExpensesQuery(telegramUserId, now.Year, now.Month), cancellationToken)),

            "/receitas" => FormatIncome(
                await _mediator.Send(new GetMonthlyIncomeQuery(telegramUserId, now.Year, now.Month), cancellationToken)),

            "/resumo" => FormatSummary(
                await _mediator.Send(new GetMonthlySummaryQuery(telegramUserId, now.Year, now.Month), cancellationToken)),

            "/ultimas" => FormatLastTransactions(
                await _mediator.Send(new GetLastTransactionsQuery(telegramUserId), cancellationToken)),

            "/configurar" => await HandleConfigureCommandAsync(text, telegramUserId, cancellationToken),

            "/start" => WelcomeText(),

            "/ajuda" => HelpText(),

            _ => "Comando nao reconhecido. Use /ajuda para ver os comandos disponiveis."
        };
    }

    private async Task HandleReportCommandAsync(string text, long chatId, long telegramUserId, CancellationToken cancellationToken)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var isPrevious = parts.Length > 1 && parts[1].Equals("passado", StringComparison.OrdinalIgnoreCase);

        var target = isPrevious ? DateTime.UtcNow.AddMonths(-1) : DateTime.UtcNow;

        var report = await _mediator.Send(
            new GenerateMonthlyReportCommand(telegramUserId, target.Year, target.Month),
            cancellationToken);

        if (report is null)
        {
            await _botClient.SendMessage(chatId,
                "Nenhuma transacao encontrada para o periodo solicitado.",
                cancellationToken: cancellationToken);
            return;
        }

        using var stream = new MemoryStream(report.Content);
        var monthLabel = target.ToString("MMMM/yyyy");

        await _botClient.SendDocument(
            chatId: new ChatId(chatId),
            document: InputFile.FromStream(stream, report.FileName),
            caption: $"Relatorio de {monthLabel}",
            cancellationToken: cancellationToken);
    }

    private async Task<string> HandleConfigureCommandAsync(string text, long telegramUserId, CancellationToken cancellationToken)
    {
        // /configurar dia 5 excel
        // /configurar dia 5 csv
        // /configurar desativar
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2 && parts[1].Equals("desativar", StringComparison.OrdinalIgnoreCase))
            return await _mediator.Send(new SetReportConfigCommand(telegramUserId, null, ReportFormat.Excel), cancellationToken);

        if (parts.Length < 4 || !parts[1].Equals("dia", StringComparison.OrdinalIgnoreCase) || !int.TryParse(parts[2], out var day))
            return "Uso:\n/configurar dia [1-28] [excel|csv]\n/configurar desativar\n\nExemplo: /configurar dia 5 excel";

        var format = parts[3].Equals("csv", StringComparison.OrdinalIgnoreCase)
            ? ReportFormat.Csv
            : ReportFormat.Excel;

        return await _mediator.Send(new SetReportConfigCommand(telegramUserId, day, format), cancellationToken);
    }

    private async Task<string> HandleTextAsync(string text, long telegramUserId, string userName, CancellationToken cancellationToken)
        => await _mediator.Send(new ProcessMessageCommand(telegramUserId, userName, text), cancellationToken);

    private static string FormatExpenses(IReadOnlyList<CategorySummaryDto> items)
    {
        if (items.Count == 0) return "Nenhum gasto registrado este mes.";

        var sb = new StringBuilder();
        sb.AppendLine("Gastos do mes:");
        sb.AppendLine();
        foreach (var item in items)
            sb.AppendLine($"{item.CategoryName}: R$ {item.Total:N2}");
        sb.AppendLine();
        sb.Append($"Total: R$ {items.Sum(i => i.Total):N2}");
        return sb.ToString();
    }

    private static string FormatIncome(IReadOnlyList<CategorySummaryDto> items)
    {
        if (items.Count == 0) return "Nenhuma receita registrada este mes.";

        var sb = new StringBuilder();
        sb.AppendLine("Receitas do mes:");
        sb.AppendLine();
        foreach (var item in items)
            sb.AppendLine($"{item.CategoryName}: R$ {item.Total:N2}");
        sb.AppendLine();
        sb.Append($"Total: R$ {items.Sum(i => i.Total):N2}");
        return sb.ToString();
    }

    private static string FormatSummary(MonthlySummaryDto summary)
    {
        var saldo = summary.Balance >= 0 ? $"+R$ {summary.Balance:N2}" : $"-R$ {Math.Abs(summary.Balance):N2}";
        return $"Resumo {summary.Month:D2}/{summary.Year}\n\n" +
               $"Receitas:  R$ {summary.TotalIncome:N2}\n" +
               $"Despesas:  R$ {summary.TotalExpense:N2}\n\n" +
               $"Saldo: {saldo}";
    }

    private static string FormatLastTransactions(IReadOnlyList<TransactionDto> transactions)
    {
        if (transactions.Count == 0) return "Nenhuma transacao registrada ainda.";

        var sb = new StringBuilder();
        sb.AppendLine($"Ultimas {transactions.Count} transacoes:");
        sb.AppendLine();
        foreach (var t in transactions)
        {
            var sinal = t.Type == TransactionType.Expense ? "-" : "+";
            sb.AppendLine($"{t.CreatedAt:dd/MM} {sinal}R${t.Amount:N2} {t.CategoryName}");
            sb.AppendLine($"  {t.Description}");
        }
        return sb.ToString().TrimEnd();
    }

    private static string WelcomeText() =>
        "Ola! Eu sou o BibbleBot, seu assistente financeiro pessoal 👋\n\n" +
        "Voce pode registrar transacoes de 3 formas:\n\n" +
        "💬 Texto\n" +
        "  \"gastei 50 no mercado\"\n" +
        "  \"recebi 5000 de salario\"\n" +
        "  \"almoco 35\"\n\n" +
        "🎤 Audio\n" +
        "  Mande um audio falando a transacao, ex:\n" +
        "  \"cem reais de gasolina\"\n\n" +
        "🧾 Foto de comprovante\n" +
        "  Tire uma foto da nota fiscal ou comprovante\n" +
        "  e eu extraio os dados automaticamente\n\n" +
        "📊 Consultas:\n" +
        "/resumo   - Saldo do mes\n" +
        "/gastos   - Gastos por categoria\n" +
        "/receitas - Receitas por categoria\n" +
        "/ultimas  - Ultimas 10 transacoes\n\n" +
        "Use /ajuda para ver todos os comandos.";

    private static string HelpText() =>
        "Comandos disponiveis:\n\n" +
        "/gastos            - Gastos do mes por categoria\n" +
        "/receitas          - Receitas do mes por categoria\n" +
        "/resumo            - Saldo do mes\n" +
        "/ultimas           - Ultimas 10 transacoes\n" +
        "/relatorio         - Relatorio do mes atual\n" +
        "/relatorio passado - Relatorio do mes anterior\n" +
        "/configurar        - Configura relatorio automatico\n\n" +
        "Para registrar uma transacao, envie uma mensagem como:\n" +
        "  mercado 150\n" +
        "  gastei 80 no almoco\n" +
        "  recebi 5000 de salario";
}
