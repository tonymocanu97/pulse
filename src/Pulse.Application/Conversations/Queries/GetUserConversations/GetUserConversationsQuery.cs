using MediatR;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.Application.Conversations.Queries.GetUserConversations
{
    public record GetUserConversationsQuery(int UserId) : IRequest<IReadOnlyList<ConversationDto>>;
}
