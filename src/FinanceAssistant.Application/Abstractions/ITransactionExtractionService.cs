using FinanceAssistant.Application.DTOs;

namespace FinanceAssistant.Application.Abstractions;

public interface ITransactionExtractionService
{
    Task<TransactionExtractionResult> ExtractAsync(string text, CancellationToken cancellationToken = default);
}
