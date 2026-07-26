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
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task AddAsync(Message message, CancellationToken ct = default) =>
            await context.Messages.AddAsync(message, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await context.SaveChangesAsync(ct);
    }
}
