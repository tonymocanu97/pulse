using MediatR;

namespace Pulse.Application.Users.Commands.SetOnlineStatus
{
    public record SetOnlineStatusCommand(int UserId, bool IsOnline) : IRequest;
}
