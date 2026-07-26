using MediatR;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.Application.Conversations.Queries.GetConversationById
{
    public enum ConversationAccessError
    {
        NotFound,
        Forbidden
    }

    public record GetConversationByIdQuery(int ConversationId, int RequestingUserId)
        : IRequest<(ConversationDto? Response, ConversationAccessError? Error)>;
}
