using Pulse.Application.Auth.DTOs;
using Pulse.Application.Common.Interfaces;
using Pulse.Domain.Entities;

namespace Pulse.Application.Auth
{
    internal static class AuthResponseFactory
    {
        public static AuthResponse Build(User user, IJwtTokenService jwtTokenService)
        {
            var (token, expiresAt) = jwtTokenService.GenerateToken(user);
            var userDto = new UserDto(user.Id, user.Username, user.Email, user.AvatarUrl, user.IsOnline, user.LastSeen);
            return new AuthResponse(token, expiresAt, userDto);
        }
    }
}
