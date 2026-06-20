using FluentValidation;

namespace FinanceAssistant.Application.Commands.SetReportConfig;

public class SetReportConfigCommandValidator : AbstractValidator<SetReportConfigCommand>
{
    public SetReportConfigCommandValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0).WithMessage("TelegramUserId inválido.");

        When(x => x.ReportDay.HasValue, () =>
        {
            RuleFor(x => x.ReportDay!.Value)
                .InclusiveBetween(1, 28).WithMessage("Dia deve ser entre 1 e 28.");
        });
    }
}
