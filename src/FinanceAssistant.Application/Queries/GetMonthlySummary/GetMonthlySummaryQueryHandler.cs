using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetMonthlySummary;

public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetMonthlySummaryQueryHandler(IUserRepository userRepository, ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is null)
            return new MonthlySummaryDto(request.Year, request.Month, 0, 0, 0);

        var transactions = await _transactionRepository.GetByUserAndMonthAsync(user.Id, request.Year, request.Month, cancellationToken);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        return new MonthlySummaryDto(request.Year, request.Month, totalIncome, totalExpense, totalIncome - totalExpense);
    }
}
