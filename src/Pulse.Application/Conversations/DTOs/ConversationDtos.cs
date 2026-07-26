using Pulse.Domain.Enums;

namespace Pulse.Application.Conversations.DTOs
{
    public record ParticipantDto(int UserId, string Username, string? AvatarUrl, bool IsOnline, DateTime? LastSeen);

    public record MessageSummaryDto(int Id, int SenderId, string SenderUsername, string Content, MessageType Type, DateTime SentAt);

    public record ConversationDto(
        int Id,
        string? Name,
        bool IsGroup,
        DateTime CreatedAt,
        IReadOnlyList<ParticipantDto> Participants,
        MessageSummaryDto? LastMessage);

    public record CreateConversationRequest(IReadOnlyList<int> ParticipantUserIds, string? Name, bool IsGroup);
}
