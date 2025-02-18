namespace ClubPadel.Models
{
    public class UserRoles
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; internal set; }
        public User User { get; internal set; }
    }
}
