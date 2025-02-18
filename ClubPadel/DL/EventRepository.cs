using ClubPadel.DL.EfCore;
using ClubPadel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace ClubPadel.DL
{
    public class EventRepository : BaseLiteDbRepository<Event>
    {
        public EventRepository(string databasePath, string collectionName) : base(databasePath, collectionName)
        {
        }
    }

    public class EventSqlRepository : EfCoreRepository<Event>
    {
        public EventSqlRepository(ApplicationDbContext context) : base(context)
        {
        }

        public new Event GetById(Guid id)
        {
            var @event = _context.Events
                .Include(e => e.Participants)
                .FirstOrDefault(e => e.Id == id);
            @event.Participants.OrderBy(p => p.CreatedAt);
            var isTracked = _context.ChangeTracker.Entries<Event>().Any(e => e.Entity.Id == id);
            Console.WriteLine($"Is event tracked? {isTracked}");
            return @event;
        }
    }

    public class ParticipantSqlRepository : EfCoreRepository<Participant>
    {
        public ParticipantSqlRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
