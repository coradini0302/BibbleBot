using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinanceAssistant.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => await _context.Transactions.AddAsync(transaction, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByUserAndMonthAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId
                     && t.CreatedAt.Year == year
                     && t.CreatedAt.Month == month)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetLastByUserAsync(
        Guid userId, int count, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
