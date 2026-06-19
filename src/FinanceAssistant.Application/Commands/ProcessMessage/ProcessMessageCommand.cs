using MediatR;

namespace FinanceAssistant.Application.Commands.ProcessMessage;

public record ProcessMessageCommand(
    long TelegramUserId,
    string UserName,
    string MessageText
) : IRequest<string>;
