using Pulse.Domain.Entities;

namespace Pulse.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
