using MediatR;
using Microsoft.Extensions.Logging;
using Pulse.Application.Auth.DTOs;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Domain.Entities;

namespace Pulse.Application.Auth.Commands.Register
{
    public class RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, (AuthResponse? Response, string? Error)>
    {
        public async Task<(AuthResponse? Response, string? Error)> Handle(RegisterCommand request, CancellationToken ct)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var username = request.Username.Trim();

            if (await userRepository.ExistsByEmailAsync(normalizedEmail, ct))
            {
                logger.LogWarning("Registration attempt with already-registered email {Email}.", normalizedEmail);
                return (null, $"An account with email '{normalizedEmail}' already exists.");
            }

            if (await userRepository.ExistsByUsernameAsync(username, ct))
            {
                logger.LogWarning("Registration attempt with already-taken username {Username}.", username);
                return (null, $"Username '{username}' is already taken.");
            }

            var user = new User
            {
                Username = username,
                Email = normalizedEmail,
                PasswordHash = passwordHasher.Hash(request.Password),
                IsOnline = false
            };

            await userRepository.AddAsync(user, ct);
            await userRepository.SaveChangesAsync(ct);

            logger.LogInformation("User {Username} registered successfully (UserId {UserId}).", username, user.Id);

            return (AuthResponseFactory.Build(user, jwtTokenService), null);
        }
    }
}
