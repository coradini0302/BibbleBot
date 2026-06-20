using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Repositories;
using MediatR;

namespace FinanceAssistant.Application.Commands.SetReportConfig;

public class SetReportConfigCommandHandler : IRequestHandler<SetReportConfigCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetReportConfigCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(SetReportConfigCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);

        if (user is null)
        {
            user = new User(request.TelegramUserId, "Usuário");
            await _userRepository.AddAsync(user, cancellationToken);
        }

        user.ConfigureReport(request.ReportDay, request.Format);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.ReportDay is null
            ? "Relatorio automatico desativado."
            : $"Configurado! Voce recebera o relatorio todo dia {request.ReportDay} no formato {request.Format}.";
    }
}
