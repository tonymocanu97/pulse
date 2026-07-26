using MediatR;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.Application.Conversations.Queries.GetConversationById
{
    public class GetConversationByIdQueryHandler(IConversationRepository conversationRepository)
        : IRequestHandler<GetConversationByIdQuery, (ConversationDto? Response, ConversationAccessError? Error)>
    {
        public async Task<(ConversationDto? Response, ConversationAccessError? Error)> Handle(
            GetConversationByIdQuery request, CancellationToken ct)
        {
            var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, ct);
            if (conversation is null)
            {
                return (null, ConversationAccessError.NotFound);
            }

            if (!conversation.Participants.Any(p => p.UserId == request.RequestingUserId))
            {
                return (null, ConversationAccessError.Forbidden);
            }

            return (ConversationDtoFactory.Build(conversation), null);
        }
    }
}
