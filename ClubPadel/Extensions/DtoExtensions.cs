using ClubPadel.DTO;
using ClubPadel.Models;

namespace ClubPadel.Extensions
{
    public static class DtoExtensions
    {
        public static EventDto ToDto(this Event item) => new EventDto
        {
            Date = item.Date,
            Id = item.Id,
            Location = item.Location.ToString(),
            Name = item.Name,
            Time = item.Time,
            Price = item.Price,
            Level = item.Level,
            Duration = item.Duration,
            SendAt = item.SendAt,
            IsOnHold = item.IsOnHold,
            MaxParticipants = item.MaxParticipants,
            Participants = [.. item.Participants.Where(p => !p.IsOnWaitList).OrderBy(p => p.CreatedAt).Select(p => p.ToDto())],
            Waitlist = [.. item.Participants.Where(p => p.IsOnWaitList).OrderBy(p => p.CreatedAt).Select(p => p.ToDto())],
            TelegramMessageId = item.TelegramMessageId
        };

        public static Event ToEntity(this EventDto dto) => new Event
        {
            Id = dto.Id,
            Name = dto.Name,
            Date = dto.Date,
            Time = dto.Time,
            Location = dto.Location,
            Price = dto.Price,
            Level = dto.Level,
            Duration = dto.Duration,
            SendAt = dto.SendAt,
            IsOnHold = dto.IsOnHold,
            MaxParticipants = dto.MaxParticipants,
            TelegramMessageId = dto.TelegramMessageId,
            Participants = dto.Participants?.Select(p => p.ToEntity()).ToList()
        };

        public static ParticipantDto ToDto(this Participant item) => new ParticipantDto
        {
            Id = item.Id,
            Name = item.Name,
            Confirmed = item.Confirmed,
            EventId = item.EventId ?? default,
            UserName = item.UserName,
        };

        public static Participant ToEntity(this ParticipantDto dto) => new Participant
        {
            Name = dto.Name,
            UserName = dto.UserName,
            Confirmed = dto.Confirmed,
            EventId = dto.EventId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
