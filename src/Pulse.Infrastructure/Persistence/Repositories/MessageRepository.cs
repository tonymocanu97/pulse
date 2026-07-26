using Microsoft.EntityFrameworkCore;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Domain.Entities;

namespace Pulse.Infrastructure.Persistence.Repositories
{
    public class MessageRepository(PulseDbContext context) : IMessageRepository
    {
        public async Task<Message?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<IReadOnlyList<Message>> GetHistoryAsync(
            int conversationId,
            int page,
            int pageSize,
            CancellationToken ct = default) =>
            await context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                    .ThenInclude(r => r.User)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task AddAsync(Message message, CancellationToken ct = default) =>
            await context.Messages.AddAsync(message, ct);

        public async Task<MessageReaction?> GetReactionAsync(int messageId, int userId, string emoji, CancellationToken ct = default) =>
            await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji, ct);

        public async Task AddReactionAsync(MessageReaction reaction, CancellationToken ct = default) =>
            await context.MessageReactions.AddAsync(reaction, ct);

        public void RemoveReaction(MessageReaction reaction) =>
            context.MessageReactions.Remove(reaction);

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await context.SaveChangesAsync(ct);
    }
}
