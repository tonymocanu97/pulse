using Microsoft.AspNetCore.SignalR;
using Pulse.Application.Common.Interfaces;

namespace Pulse.API.Hubs
{
    public class ChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
    {
        public Task UserOnlineChanged(int userId, bool isOnline, DateTime? lastSeen, CancellationToken ct = default) =>
            hubContext.Clients.All.SendAsync(
                isOnline ? "UserOnline" : "UserOffline",
                userId,
                lastSeen,
                cancellationToken: ct);
    }
}
