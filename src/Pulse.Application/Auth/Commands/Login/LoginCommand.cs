using MediatR;
using Pulse.Application.Auth.DTOs;

namespace Pulse.Application.Auth.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<AuthResponse?>;
}
