using FinanceAssistant.Application.DTOs;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlyExpenses;

public record GetMonthlyExpensesQuery(long TelegramUserId, int Year, int Month) : IRequest<IReadOnlyList<CategorySummaryDto>>;
