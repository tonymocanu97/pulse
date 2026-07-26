using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Messages.Commands.SendMessage;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Entities;
using Pulse.Domain.Enums;

namespace Pulse.UnitTests.Messages
{
    public class SendMessageCommandHandlerTests
    {
        private readonly Mock<IConversationRepository> _conversationRepository = new();
        private readonly Mock<IMessageRepository> _messageRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IChatNotificationService> _notificationService = new();
        private readonly SendMessageCommandHandler _handler;

        private readonly User _sender = new() { Id = 1, Username = "alice", Email = "alice@example.com" };

        public SendMessageCommandHandlerTests()
        {
            _handler = new SendMessageCommandHandler(
                _conversationRepository.Object,
                _messageRepository.Object,
                _userRepository.Object,
                _notificationService.Object);
        }

        [Fact]
        public async Task Handle_ParticipantSendsMessage_PersistsAndBroadcasts()
        {
            _conversationRepository
                .Setup(r => r.IsParticipantAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(_sender);

            var command = new SendMessageCommand(1, 1, "Hello there", MessageType.Text);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            error.Should().BeNull();
            response.Should().NotBeNull();
            response!.Content.Should().Be("Hello there");
            response.SenderUsername.Should().Be("alice");

            _messageRepository.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationService.Verify(
                n => n.MessageReceived(1, It.IsAny<MessageDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonParticipant_ReturnsErrorWithoutPersisting()
        {
            _conversationRepository
                .Setup(r => r.IsParticipantAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new SendMessageCommand(1, 1, "Hello there", MessageType.Text);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(SendMessageError.NotParticipant);

            _messageRepository.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyContent_ReturnsErrorWithoutCheckingParticipation()
        {
            var command = new SendMessageCommand(1, 1, "   ", MessageType.Text);

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(SendMessageError.EmptyContent);

            _conversationRepository.Verify(
                r => r.IsParticipantAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
