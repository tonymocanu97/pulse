using Microsoft.EntityFrameworkCore;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Domain.Entities;

namespace Pulse.Infrastructure.Persistence.Repositories
{
    public class UserRepository(PulseDbContext context) : IUserRepository
    {
        public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
            await context.Users.AnyAsync(u => u.Email == email, ct);

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
            await context.Users.AnyAsync(u => u.Username == username, ct);

        public async Task<IReadOnlyList<User>> SearchAsync(string query, int take, CancellationToken ct = default) =>
            await context.Users
                .Where(u => EF.Functions.ILike(u.Username, $"%{query}%"))
                .OrderBy(u => u.Username)
                .Take(take)
                .ToListAsync(ct);

        public async Task AddAsync(User user, CancellationToken ct = default) =>
            await context.Users.AddAsync(user, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await context.SaveChangesAsync(ct);
    }
}
