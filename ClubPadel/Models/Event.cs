using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.Models
{
    public class Event : EntityBase
    {
        public int TelegramMessageId { get; internal set; }

        public DateTimeOffset Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public bool IsOnWaitList { get; set; }

        public virtual List<Participant> Participants { get; set; } = new List<Participant>();
    }
}
