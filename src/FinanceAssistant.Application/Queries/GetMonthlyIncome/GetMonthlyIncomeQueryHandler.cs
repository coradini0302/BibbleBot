using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlyIncome;

public class GetMonthlyIncomeQueryHandler : IRequestHandler<GetMonthlyIncomeQuery, IReadOnlyList<CategorySummaryDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetMonthlyIncomeQueryHandler(IUserRepository userRepository, ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<CategorySummaryDto>> Handle(GetMonthlyIncomeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is null) return [];

        var transactions = await _transactionRepository.GetByUserAndMonthAsync(user.Id, request.Year, request.Month, cancellationToken);

        return transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummaryDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Total)
            .ToList();
    }
}
