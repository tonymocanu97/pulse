using MediatR;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.Application.Conversations.Commands.CreateConversation
{
    public class CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IChatNotificationService notificationService)
        : IRequestHandler<CreateConversationCommand, (ConversationDto? Response, string? Error)>
    {
        public async Task<(ConversationDto? Response, string? Error)> Handle(CreateConversationCommand request, CancellationToken ct)
        {
            var participantIds = request.ParticipantUserIds.Append(request.CreatorUserId).Distinct().ToList();

            if (participantIds.Count < 2)
            {
                return (null, "A conversation requires at least one other participant.");
            }

            foreach (var userId in participantIds)
            {
                if (await userRepository.GetByIdAsync(userId, ct) is null)
                {
                    return (null, $"User with id {userId} does not exist.");
                }
            }

            if (!request.IsGroup)
            {
                if (participantIds.Count != 2)
                {
                    return (null, "A direct conversation must have exactly two participants.");
                }

                var existing = await conversationRepository.FindDirectConversationAsync(participantIds[0], participantIds[1], ct);
                if (existing is not null)
                {
                    return (ConversationDtoFactory.Build(existing), null);
                }
            }

            var conversation = new Conversation
            {
                IsGroup = request.IsGroup,
                Name = request.IsGroup ? request.Name : null,
                Participants = participantIds.Select(userId => new Participant { UserId = userId }).ToList()
            };

            await conversationRepository.AddAsync(conversation, ct);
            await conversationRepository.SaveChangesAsync(ct);

            var created = await conversationRepository.GetByIdAsync(conversation.Id, ct);
            var dto = ConversationDtoFactory.Build(created!);

            await notificationService.ConversationCreated(participantIds, dto, ct);

            return (dto, null);
        }
    }
}
