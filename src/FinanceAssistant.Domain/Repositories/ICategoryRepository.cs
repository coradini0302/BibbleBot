using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByNameAndTypeAsync(string name, TransactionType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllByTypeAsync(TransactionType type, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
