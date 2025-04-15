using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ClubPadel.Services
{

    public class TelegramBotService : ITelegramBotService
    {
        private static readonly string _token = Environment.GetEnvironmentVariable("TgBotToken");
        private static readonly string _chatId = Environment.GetEnvironmentVariable("TgChatId");



        private readonly HttpClient _httpClient;
        private readonly ITelegramBotClient _botClient;

        //TODO: Change to thread safe.
        private static readonly Dictionary<long, List<string>> Likes = new();
        private static readonly Dictionary<long, long> WaitingForParticipant = new();
        private readonly EventService _eventService;

        public TelegramBotService(ITelegramBotClient botClient, EventService eventService)
        {
            //TODO: di or tg lib.
            _httpClient = new HttpClient();
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task SendMessageAsync(long userId, string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            await _botClient.SendMessage(userId, text);
        }

        public async Task HandleWebhookAsync(JsonDocument update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));

            if (update.RootElement.TryGetProperty("callback_query", out var callbackQuery))
            {
                await HandleCallbackQueryAsync(callbackQuery);
            }
            else if (update.RootElement.TryGetProperty("message", out var message))
            {
                await HandleMessageAsync(message);
            }
        }

        private async Task HandleCallbackQueryAsync(JsonElement callbackQuery)
        {
            var userId = callbackQuery.GetProperty("from").GetProperty("id").GetInt64();
            var userName = callbackQuery.GetProperty("from").GetProperty("username").GetString();
            var messageId = callbackQuery.GetProperty("message").GetProperty("message_id").GetInt32();
            var callbackData = callbackQuery.GetProperty("data").GetString();

            switch (callbackData)
            {
                case "add_participant":
                    await SendMessageAsync(userId, "✏️ Enter participant name:");
                    WaitingForParticipant[userId] = messageId;
                    break;

                    //case "like_event":
                    //    await LikeEventAsync(messageId, userName);
                    //    break;

                    //case "unLike_event":
                    //    await UnlikeEventAsync(messageId, userName + "_removed");
                    //    break;
                    
                case "join_event":
                    await _eventService.AddParticipant(messageId, userName);
                    break;
                case "join_event_confirmed":
                    await _eventService.AddParticipant(messageId, userName, true);
                    break;
                case "exit_event":
                    await _eventService.RemoveParticipant(messageId, userName);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown callback data: {callbackData}");
            }
        }

        private async Task HandleMessageAsync(JsonElement message)
        {
            var userId = message.GetProperty("from").GetProperty("id").GetInt64();
            if (WaitingForParticipant.TryGetValue(userId, out var messageId))
            {
                var participantName = message.GetProperty("text").GetString();
                await AddParticipantAsync(messageId, participantName);
                WaitingForParticipant.Remove(userId);
            }
        }

        public async Task SendEventMessageAsync()
        {
            var url = $"https://api.telegram.org/bot{_token}/sendMessage";

            var payload = new
            {
                chat_id = _chatId,
                text = "🎾 New Event! Click ❤️ to like!",
                reply_markup = new
                {
                    inline_keyboard = new[]
                    {
                        new[]
                        {
                            new { text = "❤️ Like", callback_data = "like_event" }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Message sent: {responseString}");
        }

        private async Task AddParticipantAsync(long messageId, string participantName)
        {
            if (!Likes.ContainsKey(messageId))
            {
                Likes[messageId] = new List<string>();
            }

            Likes[messageId].Add(participantName);
            await UpdateMessageWithLikesAsync(messageId);
        }

        private async Task UpdateMessageWithLikesAsync(long messageId)
        {
            try
            {
                var chatId = new ChatId(_chatId);
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("❤️ Like", "like_event"),
                        InlineKeyboardButton.WithCallbackData("UnLike", "unLike_event")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("📥 Add Participant", "add_participant")
                    }
                });

                var likeList = Likes.TryGetValue(messageId, out var likes) && likes.Count > 0
                    ? string.Join("\n", likes)
                    : "No likes yet.";

                var text = $"🎾 New Event! Click ❤️ to like!\n\n👍 *Likes:* \n{likeList}";
  
                await _botClient.EditMessageText(chatId, (int)messageId, text, ParseMode.Markdown, replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating message: {ex.Message}");
            }
        }
    }

    public interface ITelegramBotService
    {
        Task SendMessageAsync(long userId, string text);
        Task HandleWebhookAsync(JsonDocument update);
        Task SendEventMessageAsync();
    }
}