using Microsoft.EntityFrameworkCore;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Domain.Entities;

namespace Pulse.Infrastructure.Persistence.Repositories
{
    public class ConversationRepository(PulseDbContext context) : IConversationRepository
    {
        public async Task<Conversation?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<IReadOnlyList<Conversation>> GetForUserAsync(int userId, CancellationToken ct = default) =>
            await context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
                .ToListAsync(ct);

        public async Task<Conversation?> FindDirectConversationAsync(int userId1, int userId2, CancellationToken ct = default) =>
            await context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => !c.IsGroup
                    && c.Participants.Count == 2
                    && c.Participants.Any(p => p.UserId == userId1)
                    && c.Participants.Any(p => p.UserId == userId2))
                .FirstOrDefaultAsync(ct);

        public async Task<Participant?> GetParticipantAsync(int conversationId, int userId, CancellationToken ct = default) =>
            await context.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId, ct);

        public async Task<bool> IsParticipantAsync(int conversationId, int userId, CancellationToken ct = default) =>
            await context.Participants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId, ct);

        public async Task AddAsync(Conversation conversation, CancellationToken ct = default) =>
            await context.Conversations.AddAsync(conversation, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await context.SaveChangesAsync(ct);
    }
}
