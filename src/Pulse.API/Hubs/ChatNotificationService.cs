using Microsoft.AspNetCore.SignalR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Messages.DTOs;

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

        public Task MessageReceived(int conversationId, MessageDto message, CancellationToken ct = default) =>
            hubContext.Clients.Group(ChatHub.ConversationGroupName(conversationId))
                .SendAsync("MessageReceived", message, cancellationToken: ct);

        public Task ConversationCreated(IReadOnlyList<int> participantUserIds, ConversationDto conversation, CancellationToken ct = default) =>
            hubContext.Clients.Groups(participantUserIds.Select(ChatHub.UserGroupName).ToList())
                .SendAsync("ConversationCreated", conversation, cancellationToken: ct);

        public Task ConversationRead(int conversationId, int userId, DateTime readAt, CancellationToken ct = default) =>
            hubContext.Clients.Group(ChatHub.ConversationGroupName(conversationId))
                .SendAsync("ConversationRead", conversationId, userId, readAt, cancellationToken: ct);
    }
}
