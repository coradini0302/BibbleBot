using MediatR;

namespace FinanceAssistant.Application.Commands.ProcessImage;

public record ProcessImageCommand(
    long TelegramUserId,
    string UserName,
    byte[] ImageBytes
) : IRequest<string>;
