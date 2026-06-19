using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string CategoryName,
    string Description,
    decimal Amount,
    TransactionType Type,
    DateTime CreatedAt
);
