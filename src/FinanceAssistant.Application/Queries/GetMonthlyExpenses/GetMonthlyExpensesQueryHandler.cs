using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlyExpenses;

public class GetMonthlyExpensesQueryHandler : IRequestHandler<GetMonthlyExpensesQuery, IReadOnlyList<CategorySummaryDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetMonthlyExpensesQueryHandler(IUserRepository userRepository, ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<CategorySummaryDto>> Handle(GetMonthlyExpensesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is null) return [];

        var transactions = await _transactionRepository.GetByUserAndMonthAsync(user.Id, request.Year, request.Month, cancellationToken);

        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummaryDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Total)
            .ToList();
    }
}
