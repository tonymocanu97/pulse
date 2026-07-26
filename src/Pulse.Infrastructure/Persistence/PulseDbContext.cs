using Microsoft.EntityFrameworkCore;
using Pulse.Domain.Entities;

namespace Pulse.Infrastructure.Persistence
{
    public class PulseDbContext(DbContextOptions<PulseDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PulseDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
