namespace ClubPadel.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<Participant> Waitlist { get; set; } = new List<Participant>();
        public int TelegramMessageId { get; internal set; }
    }
}
