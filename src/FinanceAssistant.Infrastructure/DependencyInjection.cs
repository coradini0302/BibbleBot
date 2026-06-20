using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Domain.Repositories;
using FinanceAssistant.Infrastructure.AI.OpenAI;
using FinanceAssistant.Infrastructure.Persistence;
using FinanceAssistant.Infrastructure.Persistence.Repositories;
using FinanceAssistant.Infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using Telegram.Bot;

namespace FinanceAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddRepositories(services);
        AddOpenAI(services, configuration);
        AddTelegram(services, configuration);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
    }

    private static void AddOpenAI(IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            apiKey = Environment.GetEnvironmentVariable("OPENAI__APIKEY");
        if (string.IsNullOrEmpty(apiKey))
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("OpenAI:ApiKey nao configurado.");

        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        services.AddScoped<ITransactionExtractionService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OpenAITransactionExtractionService>>();
            var chatClient = new OpenAIClient(apiKey).GetChatClient(model);
            return new OpenAITransactionExtractionService(chatClient, logger);
        });
    }

    private static void AddTelegram(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITelegramBotClient>(_ =>
        {
            var token = configuration["Telegram:BotToken"]
                ?? throw new InvalidOperationException("Telegram:BotToken nao configurado.");
            return new TelegramBotClient(token);
        });

        services.AddScoped<TelegramUpdateHandler>();
    }
}
