using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceAssistant.Infrastructure.Persistence;

// Usado apenas em design-time pelo dotnet-ef para gerar migrations.
// Nunca é chamado em runtime — a connection string real vem do appsettings/env.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=financeassistant;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
