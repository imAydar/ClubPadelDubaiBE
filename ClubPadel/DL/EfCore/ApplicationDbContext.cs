using ClubPadel.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubPadel.DL.EfCore
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Location> Locations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Define Primary Keys
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
            modelBuilder.Entity<Event>().HasKey(e => e.Id);
            modelBuilder.Entity<Participant>().HasKey(p => p.Id);
            modelBuilder.Entity<Location>().HasKey(l => l.Id);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);  // ✅ Ensure proper foreign key constraint

            // Configure Many-to-Many Relationship (User <-> Roles)
            modelBuilder.Entity<UserRoles>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One Relationship (Event -> Location)
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Event)
                .WithMany()
                .HasForeignKey(l => l.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for Performance
            modelBuilder.Entity<Event>().HasIndex(e => e.Date);
            modelBuilder.Entity<Location>().HasIndex(l => new { l.Latitude, l.Longitude }).IsUnique();
        }

    }

}
