using Pulse.Domain.Entities;

namespace Pulse.Application.Common.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
        Task<IReadOnlyList<User>> SearchAsync(string query, int take, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
