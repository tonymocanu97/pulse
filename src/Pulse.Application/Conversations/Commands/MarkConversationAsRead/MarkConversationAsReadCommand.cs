using MediatR;

namespace Pulse.Application.Conversations.Commands.MarkConversationAsRead
{
    public record MarkConversationAsReadCommand(int ConversationId, int UserId) : IRequest;
}
