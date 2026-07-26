using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;
using Pulse.Domain.Enums;

namespace Pulse.Application.Messages.Commands.SendMessage
{
    public class SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IChatNotificationService notificationService)
        : IRequestHandler<SendMessageCommand, (MessageDto? Response, SendMessageError? Error)>
    {
        public async Task<(MessageDto? Response, SendMessageError? Error)> Handle(SendMessageCommand request, CancellationToken ct)
        {
            var content = request.Content.Trim();
            var hasAttachment = !string.IsNullOrWhiteSpace(request.AttachmentUrl);

            if (request.Type == MessageType.Text)
            {
                if (string.IsNullOrEmpty(content))
                {
                    return (null, SendMessageError.EmptyContent);
                }
            }
            else if (!hasAttachment)
            {
                return (null, SendMessageError.MissingAttachment);
            }

            if (!await conversationRepository.IsParticipantAsync(request.ConversationId, request.SenderId, ct))
            {
                return (null, SendMessageError.NotParticipant);
            }

            var sender = await userRepository.GetByIdAsync(request.SenderId, ct);

            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Sender = sender!,
                Content = content,
                Type = request.Type,
                AttachmentUrl = hasAttachment ? request.AttachmentUrl : null,
                AttachmentFileName = hasAttachment ? request.AttachmentFileName : null,
                AttachmentSizeBytes = hasAttachment ? request.AttachmentSizeBytes : null
            };

            await messageRepository.AddAsync(message, ct);
            await messageRepository.SaveChangesAsync(ct);

            var dto = MessageDtoFactory.Build(message);

            await notificationService.MessageReceived(request.ConversationId, dto, ct);

            return (dto, null);
        }
    }
}
