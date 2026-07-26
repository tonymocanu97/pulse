using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.Application.Messages.Commands.ToggleMessageReaction
{
    public class ToggleMessageReactionCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IChatNotificationService notificationService)
        : IRequestHandler<ToggleMessageReactionCommand, (IReadOnlyList<MessageReactionDto>? Response, ToggleReactionError? Error)>
    {
        public async Task<(IReadOnlyList<MessageReactionDto>? Response, ToggleReactionError? Error)> Handle(
            ToggleMessageReactionCommand request, CancellationToken ct)
        {
            var emoji = request.Emoji.Trim();
            if (string.IsNullOrEmpty(emoji))
            {
                return (null, ToggleReactionError.EmptyEmoji);
            }

            var message = await messageRepository.GetByIdAsync(request.MessageId, ct);
            if (message is null)
            {
                return (null, ToggleReactionError.MessageNotFound);
            }

            if (!await conversationRepository.IsParticipantAsync(message.ConversationId, request.UserId, ct))
            {
                return (null, ToggleReactionError.NotParticipant);
            }

            var existing = await messageRepository.GetReactionAsync(request.MessageId, request.UserId, emoji, ct);
            if (existing is not null)
            {
                messageRepository.RemoveReaction(existing);
            }
            else
            {
                var user = await userRepository.GetByIdAsync(request.UserId, ct);
                await messageRepository.AddReactionAsync(new MessageReaction
                {
                    MessageId = request.MessageId,
                    UserId = request.UserId,
                    User = user!,
                    Emoji = emoji
                }, ct);
            }

            await messageRepository.SaveChangesAsync(ct);

            var updated = await messageRepository.GetByIdAsync(request.MessageId, ct);
            var reactions = updated!.Reactions
                .Select(r => new MessageReactionDto(r.Id, r.UserId, r.User.Username, r.Emoji))
                .ToList();

            await notificationService.MessageReactionChanged(message.ConversationId, request.MessageId, reactions, ct);

            return (reactions, null);
        }
    }
}
