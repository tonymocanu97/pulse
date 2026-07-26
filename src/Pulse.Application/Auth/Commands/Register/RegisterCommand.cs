using MediatR;
using Pulse.Application.Auth.DTOs;

namespace Pulse.Application.Auth.Commands.Register
{
    public record RegisterCommand(string Username, string Email, string Password)
        : IRequest<(AuthResponse? Response, string? Error)>;
}
