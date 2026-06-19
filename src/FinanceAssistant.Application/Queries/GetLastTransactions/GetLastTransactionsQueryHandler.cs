using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetLastTransactions;

public class GetLastTransactionsQueryHandler : IRequestHandler<GetLastTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetLastTransactionsQueryHandler(IUserRepository userRepository, ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(GetLastTransactionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is null) return [];

        var transactions = await _transactionRepository.GetLastByUserAsync(user.Id, request.Count, cancellationToken);

        return transactions
            .Select(t => new TransactionDto(t.Id, t.Category.Name, t.Description, t.Amount, t.Type, t.CreatedAt))
            .ToList();
    }
}
