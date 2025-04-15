using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.Models
{
    public class User : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }

        public List<UserRoles> UserRoles { get; set; }

        public long TelegramId { get; set; }
    }
}
