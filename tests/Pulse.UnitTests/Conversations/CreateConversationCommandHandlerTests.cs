using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Conversations.Commands.CreateConversation;
using Pulse.Application.Conversations.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.UnitTests.Conversations
{
    public class CreateConversationCommandHandlerTests
    {
        private readonly Mock<IConversationRepository> _conversationRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IChatNotificationService> _notificationService = new();
        private readonly CreateConversationCommandHandler _handler;

        private readonly User _creator = new() { Id = 1, Username = "alice", Email = "alice@example.com" };
        private readonly User _other = new() { Id = 2, Username = "bob", Email = "bob@example.com" };

        public CreateConversationCommandHandlerTests()
        {
            _handler = new CreateConversationCommandHandler(
                _conversationRepository.Object,
                _userRepository.Object,
                _notificationService.Object);

            _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(_creator);
            _userRepository.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(_other);
        }

        [Fact]
        public async Task Handle_NewDirectConversation_CreatesAndBroadcasts()
        {
            _conversationRepository
                .Setup(r => r.FindDirectConversationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Conversation?)null);

            var populated = new Conversation
            {
                Id = 99,
                IsGroup = false,
                Participants =
                [
                    new Participant { UserId = 1, User = _creator },
                    new Participant { UserId = 2, User = _other }
                ]
            };
            _conversationRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(populated);

            var command = new CreateConversationCommand(1, [2], null, false);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            error.Should().BeNull();
            response.Should().NotBeNull();
            response!.Participants.Should().HaveCount(2);

            _conversationRepository.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
            _conversationRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notificationService.Verify(
                n => n.ConversationCreated(It.IsAny<IReadOnlyList<int>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingDirectConversation_ReusesItWithoutCreatingDuplicate()
        {
            var existing = new Conversation
            {
                Id = 42,
                IsGroup = false,
                Participants =
                [
                    new Participant { UserId = 1, User = _creator },
                    new Participant { UserId = 2, User = _other }
                ]
            };
            _conversationRepository
                .Setup(r => r.FindDirectConversationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var command = new CreateConversationCommand(1, [2], null, false);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            error.Should().BeNull();
            response!.Id.Should().Be(42);

            _conversationRepository.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoOtherParticipants_ReturnsErrorWithoutCreating()
        {
            var command = new CreateConversationCommand(1, [1], null, false);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            response.Should().BeNull();
            error.Should().NotBeNullOrEmpty();

            _conversationRepository.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
