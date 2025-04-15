using ClubPadel.DL;
using ClubPadel.DTO;
using ClubPadel.Extensions;
using ClubPadel.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ClubPadel.Services
{
    public class EventService
    {
        private const int MaxParticipants = 5;
        private static readonly long ChatId = Convert.ToInt64(Environment.GetEnvironmentVariable("TgChatId"));

        private readonly ITelegramBotClient _telegramBotClient;
        private readonly EventSqlRepository _repository;
        private readonly ParticipantSqlRepository _participantRepository;
        private readonly UserRepository _userRepository;

        private readonly ILogger<EventService> _log;

        public EventService(ITelegramBotClient telegramBotClient, EventSqlRepository repository,
            ParticipantSqlRepository participantRepository, UserRepository userRepository, ILogger<EventService> log)
        {
            _telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _participantRepository = participantRepository;
            _userRepository = userRepository;
            _log = log;
        }

        public async Task AddParticipant(int messageId, long telegramId, string userName, string firstName, string lastName, bool confirmed = false)
        {
            // Use a default or configured roleId as needed
            Guid defaultRoleId = Guid.NewGuid(); // Replace with your actual default role id logic
            var user = await _userRepository.SaveUserIfNotExists(telegramId, userName, firstName, lastName, defaultRoleId);

            var eventId = _repository.GetByMessageId(messageId).Id;
            var participant = new Participant()
            {
                Id = Guid.NewGuid(),
                Name = user.Name,
                UserName = userName,
                UserId = user.Id,
                Confirmed = confirmed
            };
            await AddParticipant(eventId, participant);
        }

        /// <summary>
        /// Adds a participant to an event and updates the Telegram message.
        /// </summary>
        public async Task AddParticipant(Guid eventId, Participant participant)
        {
            // Fetch the event from the repository
            var eventItem = _repository.GetById(eventId);
            if (eventItem == null)
            {
                Console.WriteLine($"Event not found for ID: {eventId}");
                return;
            }

            // Check if the user is already a participant
            var participantEntity = eventItem.Participants.FirstOrDefault(p => p.Name == participant.Name);
            //    eventItem.Waitlist.Any(p => p.Name == participant.Name);

            if (participantEntity == null || participantEntity?.Confirmed != participant.Confirmed)
            {
                if (eventItem.Participants.Count >= MaxParticipants)
                {
                    participant.IsOnWaitList = true;
                }

                participant.EventId = eventId;
                //eventItem.Participants.Add(participant);
                await _participantRepository.Upsert(participant);
            }
            else
            {
                Console.WriteLine($"User {participant.Name} is already a participant or on the waitlist.");
            }
            await UpdateMessageWithParticipantsAsync(eventItem);
        }

        /// <summary>
        /// Updates the Telegram message with the current list of participants and waitlist.
        /// </summary>
        private async Task UpdateMessageWithParticipantsAsync(Event eventItem)
        {
            var message = GetHeaderText(eventItem);

            message = SetParticipants(eventItem, message);

            var inlineKeyboard = new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData("Записаться ⏳", "join_event"),
                    InlineKeyboardButton.WithCallbackData("Записаться ✅", "join_event_confirmed")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("Освободить место ❌", "exit_event")
                ]
            ]);

            var chatId = new ChatId(ChatId);
            //var locationMessage = await _telegramBotClient.SendLocation(
            //    chatId: chatId,
            //    latitude: 0,
            //    longitude: 0
            //);
            try
            {
                await _telegramBotClient.EditMessageText(
                    chatId: chatId,
                    messageId: eventItem.TelegramMessageId,
                    text: EscapeMarkdown(message.ToString()),
                    parseMode: ParseMode.MarkdownV2,
                    default, default,
                    replyMarkup: inlineKeyboard
                );
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("there is no text in the message to edit") &&
                    !ex.Message.Contains("message is not modified"))
                {
                    throw;
                }
            }

        }

        private static StringBuilder SetParticipants(Event eventItem, StringBuilder message)
        {
            var players = eventItem.Participants
                .OrderBy(p => p.CreatedAt)
                .Take(eventItem.MaxParticipants)
                .ToList();

            if (players?.Count == 0)
            {
                message.AppendLine("No participants yet.");
            }
            else
            {
                for (int i = 0; i < players?.Count; i++)
                {
                    var participant = players[i];
                    var confirmed = participant.Confirmed ? "✅" : "⏳";
                    message.AppendLine($"🎾{i + 1}. {participant.Name} {confirmed}");
                }
            }

            if (eventItem.Participants.Count > eventItem.MaxParticipants)
            {
                var waitlist = eventItem.Participants
                    .Except(players)
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

                if (waitlist?.Count > 0)
                {
                    message.AppendLine("\n📌 Waitlist:");
                    for (int i = 0; i < waitlist.Count; i++)
                    {
                        var participant = waitlist[i];
                        var confirmed = participant.Confirmed ? "✅" : "⏳";
                        message.AppendLine($"{i + 1}. {participant.Name} {confirmed}");
                    }
                }
            }
            return message;
        }

        private static StringBuilder GetHeaderText(Event eventItem)
        {
            var message = new StringBuilder();
            message.AppendLine($"🎾 Присоединяйтесь к игре!");
            message.AppendLine($"🎾 {eventItem.Name}\n");
            message.AppendLine($"📅 {eventItem.Date.DateTime.ToShortDateString()}");
            if (eventItem.Duration.HasValue)
            {
                var duration = GetTimeText(eventItem.Date, eventItem.Duration.Value);
                message.AppendLine(duration);
            }
            message.AppendLine($"📍 {eventItem.Location}");
            if (eventItem.Price != null)
            {
                message.AppendLine($"💰 {eventItem.Price.ToString()} AED | оплата перед игрой на ресепшен\n");
            }
            if (eventItem.MaxParticipants > 0)
            {
                message.AppendLine($"Доступно мест {eventItem.MaxParticipants - (eventItem.GetParticipantsCount())} из {eventItem.MaxParticipants}\n");
            }
            message.AppendLine("👥 Participants:");
            return message;
        }

        private static string GetTimeText(DateTimeOffset date, TimeSpan duration)
        {
            DateTimeOffset end = date + duration;

            double hours = duration.TotalHours;
            string formattedHours = hours % 1 == 0
                ? $"{(int)hours} час{(hours == 1 ? "" : "а")}"
                : $"{hours:0.##} часа"; // You can localize this more precisely if needed

            string message = $"{date:HH:mm}–{end:HH:mm} ({formattedHours})";
            return message;
        }
        /// <summary>
        /// Creates a new event and sends a Telegram message.
        /// </summary>
        public async Task<EventDto> Create(EventDto eventDto)
        {
            var eventItem = eventDto.ToEntity();
            eventItem.Date = eventItem.Date.ToUniversalTime();
            eventItem.SendAt = eventItem.SendAt?.ToUniversalTime();

            var message = GetHeaderText(eventItem);
            message = SetParticipants(eventItem, message);
            var inlineKeyboard = new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData("Записаться ⏳", "join_event"),
                    InlineKeyboardButton.WithCallbackData("Записаться ✅", "join_event_confirmed")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("Освободить место ❌", "exit_event")
                ]
            ]);

            var sentMessage = await _telegramBotClient.SendMessage(
                    chatId: ChatId,
                    text: EscapeMarkdown(message.ToString()),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: inlineKeyboard
                );

            // Store the Telegram message ID in the event
            eventItem.TelegramMessageId = sentMessage.MessageId;

            // Save the event to the repository
            _repository.Save(eventItem);

            return eventItem.ToDto();
        }

        public async Task AddParticipant(Guid eventId, ParticipantDto participantDto)
        {
            var participant = participantDto.ToEntity();
            await AddParticipant(eventId, participant);
        }

        /// <summary>
        /// Confirms a participant for an event.
        /// </summary>
        public async Task ConfirmParticipant(Guid eventId, Guid participantId, bool confirmed, Guid userId)
        {
            //var user = 
            //if (participantId != userId && userId != )
            var eventItem = _repository.GetById(eventId);
            if (eventItem == null) return;

            var participant = eventItem.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant != null)
            {
                participant.Confirmed = confirmed;
                _repository.Save(eventItem); // Save changes to the repository
                await UpdateMessageWithParticipantsAsync(eventItem);
            }

        }

        public async Task RemoveParticipant(int messageId, string? userName)
        {
            var eventItem = _repository.GetByMessageId(messageId);
            var user = eventItem.Participants.FirstOrDefault(p => p.UserName == userName); //?? eventItem.Waitlist.FirstOrDefault(p => p.UserName == userName);

            if (user == null)
            {
                Console.WriteLine($"Couldnt find the user with userName:{userName} in event eventId: {eventItem.Id} with messageId {messageId}");
                _log.LogError($"Couldnt find the user with userName:{userName} in event eventId: {eventItem.Id} with messageId {messageId}");
                return;
            }
            Console.WriteLine($"eventId: {eventItem.Id} with userId {user.Id}");
            await RemoveParticipant(eventItem.Id, user.Id);
        }

        public async Task RemoveParticipant(Guid eventId, Guid participantId)
        {
            var eventItem = _repository.GetById(eventId);
            var user = eventItem.Participants.FirstOrDefault(p => p.Id == participantId); //?? eventItem.Waitlist.FirstOrDefault(p => p.UserName == userName);



            //if (user != null)
            //{
            //    await RemoveParticipant(eventItem.Id, user.Id);
            //    return;
            //}
            //else
            //{
            //    user = eventItem.Waitlist.FirstOrDefault(p => p.UserName == userName);
            //    if (user != null)
            //    {
            //        await RemoveWaiter(eventItem.Id, user.Id);
            //        return;
            //    }
            //}
            eventItem.Participants.RemoveAll(p => p.Id == user.Id);
            if (!user.IsOnWaitList)
            {
                var next = eventItem.Participants.OrderBy(p => p.CreatedAt).FirstOrDefault(p => p.IsOnWaitList);
                if (next != null)
                {
                    next.IsOnWaitList = false;
                }
            }

            _repository.Save(eventItem); // Save changes to the repository
            await UpdateMessageWithParticipantsAsync(eventItem);

            Console.WriteLine($"User {user.UserName} removed from event {eventItem.Name}");
        }

        ///// <summary>
        ///// Removes a participant from an event.
        ///// </summary>
        //public async Task RemoveParticipant(Guid eventId, Guid participantId)
        //{
        //    var eventItem = _repository.GetById(eventId);
        //    if (eventItem == null) return;

        //    eventItem.Participants.RemoveAll(p => p.Id == participantId);

        //    if (eventItem.Waitlist?.Count > 0)
        //    {
        //        var nextInLine = eventItem.Waitlist[0];
        //        eventItem.Participants.Add(nextInLine);
        //        eventItem.Waitlist.RemoveAt(0);
        //    }

        //    _repository.Save(eventItem); // Save changes to the repository
        //    await UpdateMessageWithParticipantsAsync(eventItem);
        //}

        //public async Task RemoveWaiter(Guid eventId, Guid participantId)
        //{
        //    var eventItem = _repository.GetById(eventId);
        //    if (eventItem == null) return;

        //    eventItem.Waitlist.RemoveAll(p => p.Id == participantId);

        //    _repository.Save(eventItem); // Save changes to the repository
        //    await UpdateMessageWithParticipantsAsync(eventItem);
        //}

        /// <summary>
        /// Gets all events.
        /// </summary>
        public IEnumerable<EventDto> GetAllEvents()
        {
            return _repository.GetAll().Select(e => e.ToDto());
        }

        /// <summary>
        /// Gets an event by ID.
        /// </summary>
        public EventDto GetEventById(Guid id)
        {
            return _repository.GetById(id).ToDto();
        }

        /// <summary>
        /// Escapes special Markdown characters in the input text.
        /// </summary>
        private string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Escape special Markdown characters
            return text
                .Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("~", "\\~")
                .Replace("`", "\\`")
                .Replace(">", "\\>")
                .Replace("#", "\\#")
                .Replace("+", "\\+")
                .Replace("-", "\\-")
                .Replace("=", "\\=")
                .Replace("|", "\\|")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace(".", "\\.")
                .Replace("!", "\\!");
        }
    }
}