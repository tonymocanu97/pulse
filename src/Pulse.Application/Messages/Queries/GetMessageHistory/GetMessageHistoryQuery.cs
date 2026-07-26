using MediatR;
using Pulse.Application.Messages.DTOs;

namespace Pulse.Application.Messages.Queries.GetMessageHistory
{
    public record GetMessageHistoryQuery(int ConversationId, int RequestingUserId, int Page, int PageSize)
        : IRequest<(IReadOnlyList<MessageDto>? Response, bool IsForbidden)>;
}
