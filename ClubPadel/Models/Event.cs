namespace ClubPadel.Models
{
    public class Event : EntityBase
    {
        public int TelegramMessageId { get; internal set; }

        public DateTimeOffset Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        //public Location? Location { get; set; }
        //public Guid? LocationId { get; set; }

        public decimal? Price { get; set; }
        public string? Level { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTimeOffset? SendAt { get; set; }
        public bool IsOnHold { get; set; }
        public int MaxParticipants { get; set; }

        public virtual List<Participant> Participants { get; set; } = [];

        public int GetParticipantsCount()
        {
            return Participants.Count > MaxParticipants ? MaxParticipants : Participants.Count;
        }
    }
}
