//using ClubPadel.Services.ClubPadel.TelegramBot;
using ClubPadel.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Telegram.Bot;

namespace ClubPadel.Controllers
{
    [ApiController]
    [Route("api/telegram")]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramBotService _service;
        public TelegramController(ITelegramBotService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonDocument update)
        {
            await _service.HandleWebhookAsync(update);
            return Ok();
        }

    }
}
