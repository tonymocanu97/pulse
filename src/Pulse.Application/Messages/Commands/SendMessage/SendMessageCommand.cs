using MediatR;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Enums;

namespace Pulse.Application.Messages.Commands.SendMessage
{
    public enum SendMessageError
    {
        EmptyContent,
        NotParticipant,
        MissingAttachment
    }

    public record SendMessageCommand(
        int ConversationId,
        int SenderId,
        string Content,
        MessageType Type,
        string? AttachmentUrl = null,
        string? AttachmentFileName = null,
        long? AttachmentSizeBytes = null)
        : IRequest<(MessageDto? Response, SendMessageError? Error)>;
}
