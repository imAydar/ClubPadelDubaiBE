namespace ClubPadel.Models
{
    public class Participant
    {
        //TODO: change to new guid with date.
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Name { get; set; }
        public bool Confirmed { get; set; } = false;
    }
}
