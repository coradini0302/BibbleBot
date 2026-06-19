using FinanceAssistant.Domain.Enums;
using MediatR;

namespace FinanceAssistant.Application.Commands.CreateTransaction;

public record CreateTransactionCommand : IRequest<Guid>
{
    public long TelegramUserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
}
