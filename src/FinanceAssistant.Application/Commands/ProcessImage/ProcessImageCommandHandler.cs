using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.Commands.CreateTransaction;
using FinanceAssistant.Domain.Enums;
using MediatR;

namespace FinanceAssistant.Application.Commands.ProcessImage;

public class ProcessImageCommandHandler : IRequestHandler<ProcessImageCommand, string>
{
    private readonly IMediator _mediator;
    private readonly ITransactionExtractionService _extractionService;

    public ProcessImageCommandHandler(IMediator mediator, ITransactionExtractionService extractionService)
    {
        _mediator = mediator;
        _extractionService = extractionService;
    }

    public async Task<string> Handle(ProcessImageCommand request, CancellationToken cancellationToken)
    {
        var extraction = await _extractionService.ExtractFromImageAsync(request.ImageBytes, cancellationToken);

        if (!extraction.Success)
            return "Nao consegui ler o comprovante. Tente uma foto mais nitida ou descreva a transacao em texto.";

        var command = new CreateTransactionCommand
        {
            TelegramUserId = request.TelegramUserId,
            UserName = request.UserName,
            CategoryName = extraction.Category,
            Description = extraction.Description,
            Amount = extraction.Amount,
            Type = extraction.Type
        };

        var transactionId = await _mediator.Send(command, cancellationToken);

        var typeEmoji = extraction.Type == TransactionType.Expense ? "✅ Gasto Registrado!" : "✅ Receita Registrada!";
        var categoryEmoji = GetCategoryEmoji(extraction.Category);
        var shortId = transactionId.ToString("N")[..6];
        var date = DateTime.Now.ToString("dd/MM/yyyy");

        return $"{typeEmoji}\n\n{categoryEmoji} {extraction.Description} ({extraction.Category})\n💸 R${extraction.Amount:N2}\n📅 {date} - #{shortId}";
    }

    private static string GetCategoryEmoji(string category) => category.ToLowerInvariant() switch
    {
        "salário" or "salario"         => "💼",
        "freelance"                    => "💻",
        "investimentos"                => "📈",
        "mercado"                      => "🛒",
        "alimentação" or "alimentacao" => "🍔",
        "combustível" or "combustivel" => "⛽",
        "lazer"                        => "🎉",
        "pets"                         => "🐾",
        "casa"                         => "🏠",
        "saúde" or "saude"             => "🏥",
        "veículo" or "veiculo"         => "🚗",
        _                              => "📦",
    };
}
