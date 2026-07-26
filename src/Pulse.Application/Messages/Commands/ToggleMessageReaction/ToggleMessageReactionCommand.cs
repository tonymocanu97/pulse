using MediatR;
using Pulse.Application.Messages.DTOs;

namespace Pulse.Application.Messages.Commands.ToggleMessageReaction
{
    public enum ToggleReactionError
    {
        EmptyEmoji,
        MessageNotFound,
        NotParticipant
    }

    public record ToggleMessageReactionCommand(int MessageId, int UserId, string Emoji)
        : IRequest<(IReadOnlyList<MessageReactionDto>? Response, ToggleReactionError? Error)>;
}
