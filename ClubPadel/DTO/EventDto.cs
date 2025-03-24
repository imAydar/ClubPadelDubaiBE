using ClubPadel.Models;
using System.ComponentModel.DataAnnotations;

namespace ClubPadel.DTO
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }

        public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
        public List<ParticipantDto> Waitlist { get; set; } = new List<ParticipantDto>();
        public int TelegramMessageId { get; internal set; }
    }
}
