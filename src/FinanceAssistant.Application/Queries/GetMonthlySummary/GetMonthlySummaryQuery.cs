using FinanceAssistant.Application.DTOs;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlySummary;

public record GetMonthlySummaryQuery(long TelegramUserId, int Year, int Month) : IRequest<MonthlySummaryDto>;
