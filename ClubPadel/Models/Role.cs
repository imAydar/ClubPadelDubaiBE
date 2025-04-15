using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.Models
{
    public class Role : EntityBase
    {
        public List<UserRoles> UserRoles { get; set; }
    }

    public enum Roles
    {
        Default,
        Admin
    }
}
