using ClubPadel.Models;

namespace ClubPadel.DL
{
    public class UserRepository : BaseLiteDbRepository<User>
    {
        public UserRepository(string databasePath, string collectionName) : base(databasePath, collectionName)
        {
        }
    }
}
