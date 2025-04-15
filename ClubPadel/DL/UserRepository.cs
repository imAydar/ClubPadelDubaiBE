using ClubPadel.DL.EfCore;
using ClubPadel.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubPadel.DL
{

    public class UserRepository : EfCoreRepository<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task SaveUser(User user, Guid roleId)
        {
            user.Id = Guid.NewGuid();

            var roles = new UserRoles { RoleId = Guid.NewGuid(), UserId = user.Id };
            user.UserRoles.Add(roles);

            _context.UserRoles.Add(roles);
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
            //var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
            //if (userEntity == null)
            //{
            //    user.Id = Guid.NewGuid();
            //    _context.Users.Add(user);
            //    await _context.SaveChangesAsync();
            //}
        }

        public async Task<User> GetUser(string userName)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }

        public async Task<User> GetUser(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<User> SaveUserIfNotExists(long telegramId, string userName, string firstName, string lastName, Guid roleId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            if (user == null)
            {
                string displayName = firstName;
                if (!string.IsNullOrEmpty(lastName))
                    displayName += " " + lastName.Substring(0, Math.Min(3, lastName.Length));

                user = new User
                {
                    Id = Guid.NewGuid(),
                    TelegramId = telegramId,
                    UserName = userName,
                    Name = displayName,
                    UserRoles = new List<UserRoles>()
                };

                var userRole = new UserRoles { RoleId = roleId, UserId = user.Id };
                user.UserRoles.Add(userRole);

                _context.UserRoles.Add(userRole);
                _context.Users.Add(user);

                await _context.SaveChangesAsync();
            }

            return user;
        }
    }
}
