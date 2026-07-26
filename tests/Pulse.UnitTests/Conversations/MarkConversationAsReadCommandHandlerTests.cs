using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.Commands.MarkConversationAsRead;
using Pulse.Domain.Entities;

namespace Pulse.UnitTests.Conversations
{
    public class MarkConversationAsReadCommandHandlerTests
    {
        private readonly Mock<IConversationRepository> _conversationRepository = new();
        private readonly Mock<IChatNotificationService> _notificationService = new();
        private readonly MarkConversationAsReadCommandHandler _handler;

        public MarkConversationAsReadCommandHandlerTests()
        {
            _handler = new MarkConversationAsReadCommandHandler(_conversationRepository.Object, _notificationService.Object);
        }

        [Fact]
        public async Task Handle_ExistingParticipant_UpdatesLastReadAtAndBroadcasts()
        {
            var participant = new Participant { Id = 1, ConversationId = 1, UserId = 1, LastReadAt = null };
            _conversationRepository
                .Setup(r => r.GetParticipantAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(participant);

            var command = new MarkConversationAsReadCommand(1, 1);

            await _handler.Handle(command, CancellationToken.None);

            participant.LastReadAt.Should().NotBeNull();
            _conversationRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notificationService.Verify(
                n => n.ConversationRead(1, 1, participant.LastReadAt!.Value, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonParticipant_DoesNothing()
        {
            _conversationRepository
                .Setup(r => r.GetParticipantAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Participant?)null);

            var command = new MarkConversationAsReadCommand(1, 1);

            await _handler.Handle(command, CancellationToken.None);

            _conversationRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notificationService.Verify(
                n => n.ConversationRead(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
