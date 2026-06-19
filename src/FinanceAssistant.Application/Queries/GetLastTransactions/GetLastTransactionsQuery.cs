using FinanceAssistant.Application.DTOs;
using MediatR;

namespace FinanceAssistant.Application.Queries.GetLastTransactions;

public record GetLastTransactionsQuery(long TelegramUserId, int Count = 10) : IRequest<IReadOnlyList<TransactionDto>>;
