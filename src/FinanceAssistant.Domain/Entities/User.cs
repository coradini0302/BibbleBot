using FinanceAssistant.Domain.Common;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Exceptions;

namespace FinanceAssistant.Domain.Entities;

public class User : Entity
{
    public long TelegramId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public int? ReportDay { get; private set; }
    public ReportFormat ReportFormat { get; private set; } = ReportFormat.Excel;

    private User() { }

    public User(long telegramId, string name)
    {
        if (telegramId <= 0)
            throw new DomainException("TelegramId deve ser positivo.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");

        TelegramId = telegramId;
        Name = name.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void ConfigureReport(int? day, ReportFormat format)
    {
        if (day.HasValue && (day < 1 || day > 28))
            throw new DomainException("Dia do relatório deve ser entre 1 e 28.");

        ReportDay = day;
        ReportFormat = format;
    }
}
