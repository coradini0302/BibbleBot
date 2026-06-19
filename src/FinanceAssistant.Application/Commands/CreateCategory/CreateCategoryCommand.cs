using FinanceAssistant.Domain.Enums;
using MediatR;

namespace FinanceAssistant.Application.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public TransactionType Type { get; init; }
}
