using Pulse.Application.Conversations.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.Application.Conversations
{
    internal static class ConversationDtoFactory
    {
        public static ConversationDto Build(Conversation conversation)
        {
            var lastMessage = conversation.Messages
                .OrderByDescending(m => m.SentAt)
                .Select(m => new MessageSummaryDto(m.Id, m.SenderId, m.Sender.Username, m.Content, m.Type, m.SentAt))
                .FirstOrDefault();

            var participants = conversation.Participants
                .Select(p => new ParticipantDto(p.UserId, p.User.Username, p.User.AvatarUrl, p.User.IsOnline, p.User.LastSeen, p.LastReadAt))
                .ToList();

            return new ConversationDto(conversation.Id, conversation.Name, conversation.IsGroup, conversation.CreatedAt, participants, lastMessage);
        }
    }
}
