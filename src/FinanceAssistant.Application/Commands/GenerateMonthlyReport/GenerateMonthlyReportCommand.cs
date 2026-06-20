using FinanceAssistant.Application.DTOs;
using MediatR;

namespace FinanceAssistant.Application.Commands.GenerateMonthlyReport;

public record GenerateMonthlyReportCommand(long TelegramUserId, int Year, int Month) : IRequest<ReportFile?>;
