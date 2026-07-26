using MediatR;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.DTOs;

namespace Pulse.Application.Messages.Queries.GetMessageHistory
{
    public class GetMessageHistoryQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
        : IRequestHandler<GetMessageHistoryQuery, (IReadOnlyList<MessageDto>? Response, bool IsForbidden)>
    {
        public async Task<(IReadOnlyList<MessageDto>? Response, bool IsForbidden)> Handle(
            GetMessageHistoryQuery request, CancellationToken ct)
        {
            if (!await conversationRepository.IsParticipantAsync(request.ConversationId, request.RequestingUserId, ct))
            {
                return (null, true);
            }

            var messages = await messageRepository.GetHistoryAsync(request.ConversationId, request.Page, request.PageSize, ct);
            return (messages.Select(MessageDtoFactory.Build).ToList(), false);
        }
    }
}
