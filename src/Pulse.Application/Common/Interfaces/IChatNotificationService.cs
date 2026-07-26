using Pulse.Application.Auth.DTOs;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Messages.DTOs;

namespace Pulse.Application.Common.Interfaces
{
    // Implemented in Pulse.API (backed by IHubContext<ChatHub>) so the Application layer
    // can push real-time events without depending on ASP.NET Core SignalR directly.
    public interface IChatNotificationService
    {
        Task UserOnlineChanged(int userId, bool isOnline, DateTime? lastSeen, CancellationToken ct = default);

        Task MessageReceived(int conversationId, MessageDto message, CancellationToken ct = default);

        Task ConversationCreated(IReadOnlyList<int> participantUserIds, ConversationDto conversation, CancellationToken ct = default);

        Task ConversationRead(int conversationId, int userId, DateTime readAt, CancellationToken ct = default);

        Task MessageReactionChanged(
            int conversationId, int messageId, IReadOnlyList<MessageReactionDto> reactions, CancellationToken ct = default);
    }
}
