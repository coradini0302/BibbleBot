using FinanceAssistant.Application.DTOs;

namespace FinanceAssistant.Application.Abstractions;

public interface IReportGeneratorService
{
    Task<ReportFile> GenerateAsync(MonthlyReportDto data, CancellationToken cancellationToken = default);
}
