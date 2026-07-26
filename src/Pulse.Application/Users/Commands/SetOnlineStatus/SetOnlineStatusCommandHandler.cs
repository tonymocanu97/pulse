using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;

namespace Pulse.Application.Users.Commands.SetOnlineStatus
{
    public class SetOnlineStatusCommandHandler(
        IUserRepository userRepository,
        IChatNotificationService notificationService) : IRequestHandler<SetOnlineStatusCommand>
    {
        public async Task Handle(SetOnlineStatusCommand request, CancellationToken ct)
        {
            var user = await userRepository.GetByIdAsync(request.UserId, ct);
            if (user is null)
            {
                return;
            }

            user.IsOnline = request.IsOnline;
            user.LastSeen = request.IsOnline ? user.LastSeen : DateTime.UtcNow;

            await userRepository.SaveChangesAsync(ct);
            await notificationService.UserOnlineChanged(user.Id, user.IsOnline, user.LastSeen, ct);
        }
    }
}
