using FinanceAssistant.Domain.Common;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Exceptions;

namespace FinanceAssistant.Domain.Entities;

public class Transaction : Entity
{
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    private Transaction() { }

    public Transaction(Guid userId, Guid categoryId, string description, decimal amount, TransactionType type)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId é obrigatório.");

        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId é obrigatório.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatória.");

        if (amount <= 0)
            throw new DomainException("Valor deve ser maior que zero.");

        UserId = userId;
        CategoryId = categoryId;
        Description = description.Trim();
        Amount = amount;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }
}
