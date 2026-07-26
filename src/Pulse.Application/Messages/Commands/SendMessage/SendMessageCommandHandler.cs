using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;

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
            if (string.IsNullOrEmpty(content))
            {
                return (null, SendMessageError.EmptyContent);
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
                Type = request.Type
            };

            await messageRepository.AddAsync(message, ct);
            await messageRepository.SaveChangesAsync(ct);

            var dto = MessageDtoFactory.Build(message);

            await notificationService.MessageReceived(request.ConversationId, dto, ct);

            return (dto, null);
        }
    }
}
