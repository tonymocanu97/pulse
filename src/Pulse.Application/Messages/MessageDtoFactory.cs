using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.Application.Messages
{
    internal static class MessageDtoFactory
    {
        public static MessageDto Build(Message message) =>
            new(
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.Sender.Username,
                message.Sender.AvatarUrl,
                message.Content,
                message.Type,
                message.SentAt,
                message.IsEdited,
                message.Reactions
                    .Select(r => new MessageReactionDto(r.Id, r.UserId, r.User.Username, r.Emoji))
                    .ToList());
    }
}
