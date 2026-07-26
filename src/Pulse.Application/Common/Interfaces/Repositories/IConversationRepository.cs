using Pulse.Domain.Entities;

namespace Pulse.Application.Common.Interfaces.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Conversation>> GetForUserAsync(int userId, CancellationToken ct = default);
        Task<Conversation?> FindDirectConversationAsync(int userId1, int userId2, CancellationToken ct = default);
        Task<Participant?> GetParticipantAsync(int conversationId, int userId, CancellationToken ct = default);
        Task<bool> IsParticipantAsync(int conversationId, int userId, CancellationToken ct = default);
        Task AddAsync(Conversation conversation, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
