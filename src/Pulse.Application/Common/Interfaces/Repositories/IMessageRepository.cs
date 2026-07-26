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
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
