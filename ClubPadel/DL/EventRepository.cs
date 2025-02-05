using ClubPadel.Models;

namespace ClubPadel.DL
{
    public class EventRepository : BaseDbRepository<Event>
    {
        public EventRepository(string databasePath, string collectionName) : base(databasePath, collectionName)
        {
        }
    }
}
