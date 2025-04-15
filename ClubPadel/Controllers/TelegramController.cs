//using ClubPadel.Services.ClubPadel.TelegramBot;
using ClubPadel.Services;
using Microsoft.AspNetCore.Authorization;
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
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telegram_updates.log");
            await System.IO.File.AppendAllTextAsync(logPath, $"{DateTime.UtcNow:O} {update.RootElement}\n");
            await _service.HandleWebhookAsync(update);
            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("perm")]
        public async Task<IActionResult> HandleWebhook()
        {
            return Ok();
        }
    }
}
