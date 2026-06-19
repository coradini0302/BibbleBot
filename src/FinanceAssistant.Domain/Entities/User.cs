using FinanceAssistant.Domain.Common;
using FinanceAssistant.Domain.Exceptions;

namespace FinanceAssistant.Domain.Entities;

public class User : Entity
{
    public long TelegramId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public User(long telegramId, string name)
    {
        if (telegramId <= 0)
            throw new DomainException("TelegramId deve ser positivo.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");

        TelegramId = telegramId;
        Name = name.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
