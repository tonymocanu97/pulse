using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.Commands.MarkConversationAsRead;
using Pulse.Application.Users.Commands.SetOnlineStatus;

namespace Pulse.API.Hubs
{
    [Authorize]
    public class ChatHub(IMediator mediator, IConversationRepository conversationRepository) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(userId));

            // Conversation-scoped groups are normally joined on demand via JoinConversation
            // (when the client opens a thread), but the sidebar's conversation list needs
            // live updates (last message preview, ordering) for every conversation the user
            // is part of, not just the one currently open - so join all of them upfront.
            var conversations = await conversationRepository.GetForUserAsync(userId);
            foreach (var conversation in conversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroupName(conversation.Id));
            }

            await mediator.Send(new SetOnlineStatusCommand(userId, true));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await mediator.Send(new SetOnlineStatusCommand(GetUserId(), false));
            await base.OnDisconnectedAsync(exception);
        }

        public Task JoinConversation(int conversationId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroupName(conversationId));

        public Task LeaveConversation(int conversationId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroupName(conversationId));

        public Task MarkAsRead(int conversationId) =>
            mediator.Send(new MarkConversationAsReadCommand(conversationId, GetUserId()));

        public Task SendTypingIndicator(int conversationId)
        {
            var username = Context.User?.Identity?.Name ?? "Someone";
            return Clients.OthersInGroup(ConversationGroupName(conversationId)).SendAsync("UserTyping", conversationId, username);
        }

        private int GetUserId() => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static string ConversationGroupName(int conversationId) => $"conversation-{conversationId}";

        public static string UserGroupName(int userId) => $"user-{userId}";
    }
}
