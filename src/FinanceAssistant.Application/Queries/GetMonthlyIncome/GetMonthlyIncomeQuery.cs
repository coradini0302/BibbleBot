using FinanceAssistant.Application.DTOs;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlyIncome;

public record GetMonthlyIncomeQuery(long TelegramUserId, int Year, int Month) : IRequest<IReadOnlyList<CategorySummaryDto>>;
