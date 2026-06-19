using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinanceAssistant.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<Category?> GetByNameAndTypeAsync(string name, TransactionType type, CancellationToken cancellationToken = default)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.Type == type, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllByTypeAsync(TransactionType type, CancellationToken cancellationToken = default)
        => await _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Categories.FindAsync([id], cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => await _context.Categories.AddAsync(category, cancellationToken);
}
