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
            return @event;
        }

        public Event GetByMessageId(int messageId)
        {
            var @event = _context.Events
                .Include(e => e.Participants)
                .FirstOrDefault(e => e.TelegramMessageId == messageId);
            @event.Participants.OrderBy(p => p.CreatedAt);
            return @event;
        }
    }

    public class ParticipantSqlRepository : EfCoreRepository<Participant>
    {
        public ParticipantSqlRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task Upsert(Participant participant)
        {
            var entity = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserName == participant.UserName && p.EventId == participant.EventId);

            if (entity != null)
            {
                // Update only needed fields
                entity.Name = participant.Name;
                entity.Confirmed = participant.Confirmed;
                entity.IsOnWaitList = participant.IsOnWaitList;
                entity.UserName = participant.UserName;
                entity.EventId = participant.EventId;
                // etc.
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Add(participant);
            }

    //        var changes = _context.ChangeTracker.Entries()
    //.Select(e => new
    //{
    //    Entity = e.Entity.GetType().Name,
    //    State = e.State,
    //    Properties = e.Properties.Where(p => p.IsModified).Select(p => new { p.Metadata.Name, p.CurrentValue })
    //});

    //        foreach (var change in changes)
    //        {
    //            Console.WriteLine($"[EF] {change.Entity} - {change.State}");
    //            foreach (var prop in change.Properties)
    //            {
    //                Console.WriteLine($"  ↳ {prop.Name} = {prop.CurrentValue}");
    //            }
    //        }

            await _context.SaveChangesAsync();

            //var entity = await _context.Participants.FirstOrDefaultAsync(p => p.UserName == participant.UserName && p.EventId == participant.EventId);

            //if (entity != null)
            //{
            //    entity = participant;
            //    _dbSet.Update(participant);
            //}
            //else
            //{
            //    _dbSet.Add(participant);
            //}

            //_context.SaveChanges();
        }

        public async Task SaveUser(User user)
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
            if (userEntity == null)
            {
                user.Id = Guid.NewGuid();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
