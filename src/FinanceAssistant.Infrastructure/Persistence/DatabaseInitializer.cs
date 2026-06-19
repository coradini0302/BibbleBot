using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceAssistant.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        await SeedCategoriesAsync(context);
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            Category.CreateDefault("Salário", TransactionType.Income),
            Category.CreateDefault("Freelance", TransactionType.Income),
            Category.CreateDefault("Investimentos", TransactionType.Income),
            Category.CreateDefault("Mercado", TransactionType.Expense),
            Category.CreateDefault("Alimentação", TransactionType.Expense),
            Category.CreateDefault("Combustível", TransactionType.Expense),
            Category.CreateDefault("Lazer", TransactionType.Expense),
            Category.CreateDefault("Pets", TransactionType.Expense),
            Category.CreateDefault("Casa", TransactionType.Expense),
            Category.CreateDefault("Saúde", TransactionType.Expense),
            Category.CreateDefault("Veículo", TransactionType.Expense),
            Category.CreateDefault("Outros", TransactionType.Expense),
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
