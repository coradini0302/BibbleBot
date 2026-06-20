using FinanceAssistant.Domain.Enums;
using MediatR;

namespace FinanceAssistant.Application.Commands.SetReportConfig;

public record SetReportConfigCommand(
    long TelegramUserId,
    int? ReportDay,
    ReportFormat Format
) : IRequest<string>;
