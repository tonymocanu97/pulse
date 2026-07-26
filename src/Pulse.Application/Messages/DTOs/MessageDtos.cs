using Pulse.Domain.Enums;

namespace Pulse.Application.Messages.DTOs
{
    public record MessageReactionDto(int Id, int UserId, string Username, string Emoji);

    public record MessageDto(
        int Id,
        int ConversationId,
        int SenderId,
        string SenderUsername,
        string? SenderAvatarUrl,
        string Content,
        MessageType Type,
        DateTime SentAt,
        bool IsEdited,
        IReadOnlyList<MessageReactionDto> Reactions,
        string? AttachmentUrl,
        string? AttachmentFileName,
        long? AttachmentSizeBytes);

    public record SendMessageRequest(
        string Content,
        MessageType Type,
        string? AttachmentUrl = null,
        string? AttachmentFileName = null,
        long? AttachmentSizeBytes = null);

    public record ToggleReactionRequest(string Emoji);
}
