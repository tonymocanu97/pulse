using MediatR;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.Application.Conversations.Queries.GetUserConversations
{
    public class GetUserConversationsQueryHandler(IConversationRepository conversationRepository)
        : IRequestHandler<GetUserConversationsQuery, IReadOnlyList<ConversationDto>>
    {
        public async Task<IReadOnlyList<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken ct)
        {
            var conversations = await conversationRepository.GetForUserAsync(request.UserId, ct);
            return conversations.Select(ConversationDtoFactory.Build).ToList();
        }
    }
}
