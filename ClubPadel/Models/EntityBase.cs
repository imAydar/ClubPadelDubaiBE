namespace ClubPadel.Models
{
    public class EntityBase
    {
        public Guid Id { get; set; } = Guid.CreateVersion7(DateTimeOffset.Now);
        public string Name { get; set; }
    }
}