using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.Commands.CreateTransaction;
using FinanceAssistant.Application.Services;
using FinanceAssistant.Domain.Enums;
using MediatR;

namespace FinanceAssistant.Application.Commands.ProcessMessage;

public class ProcessMessageCommandHandler : IRequestHandler<ProcessMessageCommand, string>
{
    private readonly IMediator _mediator;
    private readonly TransactionParserService _parserService;
    private readonly ITransactionExtractionService _extractionService;

    public ProcessMessageCommandHandler(
        IMediator mediator,
        TransactionParserService parserService,
        ITransactionExtractionService extractionService)
    {
        _mediator = mediator;
        _parserService = parserService;
        _extractionService = extractionService;
    }

    public async Task<string> Handle(ProcessMessageCommand request, CancellationToken cancellationToken)
    {
        string categoryName;
        string description;
        decimal amount;
        TransactionType type;

        var parsed = _parserService.TryParse(request.MessageText);

        if (parsed is not null)
        {
            categoryName = parsed.CategoryName;
            description = parsed.Description;
            amount = parsed.Amount;
            type = parsed.Type;
        }
        else
        {
            var extraction = await _extractionService.ExtractAsync(request.MessageText, cancellationToken);

            if (!extraction.Success)
                return "Nao consegui entender essa transacao. Tente: \"mercado 50\" ou \"gastei 100 no almoco\".";

            categoryName = extraction.Category;
            description = extraction.Description;
            amount = extraction.Amount;
            type = extraction.Type;
        }

        var command = new CreateTransactionCommand
        {
            TelegramUserId = request.TelegramUserId,
            UserName = request.UserName,
            CategoryName = categoryName,
            Description = description,
            Amount = amount,
            Type = type
        };

        await _mediator.Send(command, cancellationToken);

        var typeLabel = type == TransactionType.Expense ? "Despesa" : "Receita";
        return $"{typeLabel} registrada!\n\nCategoria: {categoryName}\nDescricao: {description}\nValor: R$ {amount:N2}";
    }
}
