using MediatR;
using Microsoft.Extensions.Logging;
using Pulse.Application.Auth.DTOs;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;

namespace Pulse.Application.Auth.Commands.Login
{
    public class LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, AuthResponse?>
    {
        public async Task<AuthResponse?> Handle(LoginCommand request, CancellationToken ct)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(normalizedEmail, ct);

            if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                logger.LogWarning("Failed login attempt for {Email}.", normalizedEmail);
                return null;
            }

            logger.LogInformation("User {Email} logged in successfully (UserId {UserId}).", normalizedEmail, user.Id);

            return AuthResponseFactory.Build(user, jwtTokenService);
        }
    }
}
