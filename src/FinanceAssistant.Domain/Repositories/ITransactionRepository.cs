using FinanceAssistant.Domain.Entities;

namespace FinanceAssistant.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByUserAndMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetLastByUserAsync(Guid userId, int count, CancellationToken cancellationToken = default);
}
