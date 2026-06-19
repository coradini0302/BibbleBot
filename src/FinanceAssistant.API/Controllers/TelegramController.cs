using FinanceAssistant.Infrastructure.Telegram;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace FinanceAssistant.API.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly TelegramUpdateHandler _handler;

    public TelegramController(TelegramUpdateHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(update, cancellationToken);
        return Ok();
    }
}
