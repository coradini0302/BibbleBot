using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using FinanceAssistant.Infrastructure.AI.OpenAI;
using FinanceAssistant.Infrastructure.BackgroundServices;
using FinanceAssistant.Infrastructure.Persistence;
using FinanceAssistant.Infrastructure.Persistence.Repositories;
using FinanceAssistant.Infrastructure.Reports;
using FinanceAssistant.Infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        AddReports(services);

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
            apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("OPENAI_KEY nao configurado.");

        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        var openAIClient = new OpenAIClient(apiKey);

        services.AddScoped<ITransactionExtractionService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OpenAITransactionExtractionService>>();
            return new OpenAITransactionExtractionService(openAIClient.GetChatClient(model), logger);
        });

        services.AddScoped<IAudioTranscriptionService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OpenAIAudioTranscriptionService>>();
            return new OpenAIAudioTranscriptionService(openAIClient.GetAudioClient("whisper-1"), logger);
        });
    }

    private static void AddTelegram(IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration["Telegram:BotToken"];
        if (string.IsNullOrEmpty(token))
            token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("TELEGRAM_TOKEN nao configurado.");

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));
        services.AddScoped<TelegramUpdateHandler>();
    }

    private static void AddReports(IServiceCollection services)
    {
        services.AddKeyedScoped<IReportGeneratorService, ExcelReportGeneratorService>(ReportFormat.Excel);
        services.AddKeyedScoped<IReportGeneratorService, CsvReportGeneratorService>(ReportFormat.Csv);
        services.AddHostedService<MonthlyReportBackgroundService>();
    }
}
