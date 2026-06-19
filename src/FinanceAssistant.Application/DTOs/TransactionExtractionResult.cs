using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Application.DTOs;

public record TransactionExtractionResult(
    bool Success,
    TransactionType Type,
    string Category,
    string Description,
    decimal Amount,
    string? ErrorMessage = null
);
