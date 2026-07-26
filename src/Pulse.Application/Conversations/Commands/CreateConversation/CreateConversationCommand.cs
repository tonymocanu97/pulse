using MediatR;
using Pulse.Application.Conversations.DTOs;

namespace Pulse.Application.Conversations.Commands.CreateConversation
{
    public record CreateConversationCommand(int CreatorUserId, IReadOnlyList<int> ParticipantUserIds, string? Name, bool IsGroup)
        : IRequest<(ConversationDto? Response, string? Error)>;
}
