using Pulse.Domain.Entities;

namespace Pulse.Application.Common.Interfaces.Repositories
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<Message>> GetHistoryAsync(
            int conversationId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task AddAsync(Message message, CancellationToken ct = default);

        Task<MessageReaction?> GetReactionAsync(int messageId, int userId, string emoji, CancellationToken ct = default);
        Task AddReactionAsync(MessageReaction reaction, CancellationToken ct = default);
        void RemoveReaction(MessageReaction reaction);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
