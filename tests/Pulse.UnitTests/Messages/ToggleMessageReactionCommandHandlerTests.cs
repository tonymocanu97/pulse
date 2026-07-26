using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.Commands.ToggleMessageReaction;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;

namespace Pulse.UnitTests.Messages
{
    public class ToggleMessageReactionCommandHandlerTests
    {
        private readonly Mock<IMessageRepository> _messageRepository = new();
        private readonly Mock<IConversationRepository> _conversationRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IChatNotificationService> _notificationService = new();
        private readonly ToggleMessageReactionCommandHandler _handler;

        private readonly User _sender = new() { Id = 1, Username = "alice", Email = "alice@example.com" };
        private readonly User _reactor = new() { Id = 2, Username = "bob", Email = "bob@example.com" };

        public ToggleMessageReactionCommandHandlerTests()
        {
            _handler = new ToggleMessageReactionCommandHandler(
                _messageRepository.Object,
                _conversationRepository.Object,
                _userRepository.Object,
                _notificationService.Object);
        }

        private Message BuildMessage(List<MessageReaction>? reactions = null) => new()
        {
            Id = 1,
            ConversationId = 1,
            SenderId = 1,
            Sender = _sender,
            Content = "hi",
            Reactions = reactions ?? []
        };

        [Fact]
        public async Task Handle_NewReaction_AddsAndBroadcasts()
        {
            _messageRepository.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildMessage())
                .ReturnsAsync(BuildMessage([new MessageReaction { Id = 5, MessageId = 1, UserId = 2, User = _reactor, Emoji = "👍" }]));

            _conversationRepository.Setup(r => r.IsParticipantAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _messageRepository.Setup(r => r.GetReactionAsync(1, 2, "👍", It.IsAny<CancellationToken>())).ReturnsAsync((MessageReaction?)null);
            _userRepository.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(_reactor);

            var (response, error) = await _handler.Handle(new ToggleMessageReactionCommand(1, 2, "👍"), CancellationToken.None);

            error.Should().BeNull();
            response.Should().ContainSingle(r => r.Emoji == "👍" && r.UserId == 2);
            _messageRepository.Verify(r => r.AddReactionAsync(It.IsAny<MessageReaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _messageRepository.Verify(r => r.RemoveReaction(It.IsAny<MessageReaction>()), Times.Never);
            _notificationService.Verify(
                n => n.MessageReactionChanged(1, 1, It.IsAny<IReadOnlyList<MessageReactionDto>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingReaction_RemovesAndBroadcasts()
        {
            var existing = new MessageReaction { Id = 5, MessageId = 1, UserId = 2, User = _reactor, Emoji = "👍" };

            _messageRepository.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildMessage([existing]))
                .ReturnsAsync(BuildMessage());

            _conversationRepository.Setup(r => r.IsParticipantAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _messageRepository.Setup(r => r.GetReactionAsync(1, 2, "👍", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

            var (response, error) = await _handler.Handle(new ToggleMessageReactionCommand(1, 2, "👍"), CancellationToken.None);

            error.Should().BeNull();
            response.Should().BeEmpty();
            _messageRepository.Verify(r => r.RemoveReaction(existing), Times.Once);
            _messageRepository.Verify(r => r.AddReactionAsync(It.IsAny<MessageReaction>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NonParticipant_ReturnsErrorWithoutPersisting()
        {
            _messageRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildMessage());
            _conversationRepository.Setup(r => r.IsParticipantAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var (response, error) = await _handler.Handle(new ToggleMessageReactionCommand(1, 2, "👍"), CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(ToggleReactionError.NotParticipant);
            _messageRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MessageNotFound_ReturnsError()
        {
            _messageRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Message?)null);

            var (response, error) = await _handler.Handle(new ToggleMessageReactionCommand(1, 2, "👍"), CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(ToggleReactionError.MessageNotFound);
        }

        [Fact]
        public async Task Handle_EmptyEmoji_ReturnsErrorWithoutLookingUpMessage()
        {
            var (response, error) = await _handler.Handle(new ToggleMessageReactionCommand(1, 2, "   "), CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(ToggleReactionError.EmptyEmoji);
            _messageRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
