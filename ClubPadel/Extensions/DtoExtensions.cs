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
            Participants = [.. item.Participants.Where(p => !p.IsOnWaitList).OrderBy(p => p.CreatedAt).Select(p => p.ToDto())],
            Waitlist = [.. item.Participants.Where(p => p.IsOnWaitList).OrderBy(p => p.CreatedAt).Select(p => p.ToDto())],

            TelegramMessageId = item.TelegramMessageId
        };

        public static ParticipantDto ToDto(this Participant item) => new ParticipantDto
        {
            Id = item.Id,
            Name = item.Name,
            Confirmed = item.Confirmed,
            EventId = item.EventId ?? default,
            UserName = item.UserName,
        };
    }
}
