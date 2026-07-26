using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;

namespace Pulse.Application.Conversations.Commands.MarkConversationAsRead
{
    public class MarkConversationAsReadCommandHandler(
        IConversationRepository conversationRepository,
        IChatNotificationService notificationService) : IRequestHandler<MarkConversationAsReadCommand>
    {
        public async Task Handle(MarkConversationAsReadCommand request, CancellationToken ct)
        {
            var participant = await conversationRepository.GetParticipantAsync(request.ConversationId, request.UserId, ct);
            if (participant is null)
            {
                return;
            }

            participant.LastReadAt = DateTime.UtcNow;
            await conversationRepository.SaveChangesAsync(ct);

            await notificationService.ConversationRead(request.ConversationId, request.UserId, participant.LastReadAt.Value, ct);
        }
    }
}
