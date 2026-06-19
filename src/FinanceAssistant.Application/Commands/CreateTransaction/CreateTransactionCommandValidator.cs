using FluentValidation;

namespace FinanceAssistant.Application.Commands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0).WithMessage("TelegramUserId deve ser positivo.");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Categoria é obrigatória.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
    }
}
