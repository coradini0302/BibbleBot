using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Commands.CreateTransaction;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransactionCommandHandler(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var user = await GetOrCreateUserAsync(request, cancellationToken);
        var category = await ResolveCategoryAsync(request.CategoryName, request.Type, cancellationToken);

        var transaction = new Transaction(user.Id, category.Id, request.Description, request.Amount, request.Type);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }

    private async Task<User> GetOrCreateUserAsync(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is not null) return user;

        user = new User(request.TelegramUserId, request.UserName);
        await _userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    private async Task<Category> ResolveCategoryAsync(string categoryName, TransactionType type, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByNameAndTypeAsync(categoryName, type, cancellationToken);
        if (category is not null) return category;

        // Categoria não encontrada: usa "Outros" como fallback
        var fallback = await _categoryRepository.GetByNameAndTypeAsync("Outros", TransactionType.Expense, cancellationToken);
        return fallback!;
    }
}
