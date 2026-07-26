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
        IReadOnlyList<MessageReactionDto> Reactions);

    public record SendMessageRequest(string Content, MessageType Type);

    public record ToggleReactionRequest(string Emoji);
}
