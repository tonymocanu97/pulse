using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.Application.Users.Commands.SetOnlineStatus;

namespace Pulse.API.Hubs
{
    [Authorize]
    public class ChatHub(IMediator mediator) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await mediator.Send(new SetOnlineStatusCommand(GetUserId(), true));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await mediator.Send(new SetOnlineStatusCommand(GetUserId(), false));
            await base.OnDisconnectedAsync(exception);
        }

        public Task JoinConversation(int conversationId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));

        public Task LeaveConversation(int conversationId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(conversationId));

        public Task SendTypingIndicator(int conversationId)
        {
            var username = Context.User?.Identity?.Name ?? "Someone";
            return Clients.OthersInGroup(GroupName(conversationId)).SendAsync("UserTyping", conversationId, username);
        }

        private int GetUserId() => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static string GroupName(int conversationId) => $"conversation-{conversationId}";
    }
}
