using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.Models
{
    public class Participant : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        public bool Confirmed { get; set; } = false;
        public Event? Event { get; set; }
        public Guid? EventId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
        //TODO: We might dont need it since we have date.
        public bool IsOnWaitList { get; set; }

        public Guid? UserId { get; set; }
        public User User { get; set; }

        //public Guid? WaitlistEventId { get; set; }
        //public Event? WaitlistEvent { get; set; }
    }
}
