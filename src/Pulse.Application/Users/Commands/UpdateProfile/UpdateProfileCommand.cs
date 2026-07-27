using MediatR;
using Pulse.Application.Auth.DTOs;

namespace Pulse.Application.Users.Commands.UpdateProfile
{
    public enum UpdateProfileError
    {
        EmptyUsername,
        UsernameTaken,
        UserNotFound
    }

    public record UpdateProfileCommand(int UserId, string Username, string? AvatarUrl)
        : IRequest<(UserDto? Response, UpdateProfileError? Error)>;
}
