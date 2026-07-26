using Pulse.Application.Auth.DTOs;

namespace Pulse.Application.Common.Interfaces
{
    // Implemented in Pulse.API (backed by IHubContext<ChatHub>) so the Application layer
    // can push real-time events without depending on ASP.NET Core SignalR directly.
    public interface IChatNotificationService
    {
        Task UserOnlineChanged(int userId, bool isOnline, DateTime? lastSeen, CancellationToken ct = default);
    }
}
