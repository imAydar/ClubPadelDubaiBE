namespace ClubPadel.DTO
{
    public class ParticipantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }

        public bool Confirmed { get; set; }
        public Guid EventId { get; set; }

        public Guid? WaitlistEventId { get; set; }

        public Guid? UserId { get; set; }
    }
}
