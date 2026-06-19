using FinanceAssistant.Domain.Common;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Exceptions;

namespace FinanceAssistant.Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category() { }

    public Category(string name, TransactionType type, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria é obrigatório.");

        Name = name.Trim();
        Type = type;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    public static Category CreateDefault(string name, TransactionType type)
        => new(name, type, isDefault: true);
}
