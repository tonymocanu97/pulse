using Pulse.Domain.Enums;

namespace Pulse.Application.Messages.DTOs
{
    public record MessageDto(
        int Id,
        int ConversationId,
        int SenderId,
        string SenderUsername,
        string? SenderAvatarUrl,
        string Content,
        MessageType Type,
        DateTime SentAt,
        bool IsEdited);

    public record SendMessageRequest(string Content, MessageType Type);
}
