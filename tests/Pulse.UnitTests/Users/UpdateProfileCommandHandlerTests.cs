using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Application.Users.Commands.UpdateProfile;
using Pulse.Domain.Entities;

namespace Pulse.UnitTests.Users
{
    public class UpdateProfileCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly UpdateProfileCommandHandler _handler;

        private readonly User _user = new() { Id = 1, Username = "alice", Email = "alice@example.com" };

        public UpdateProfileCommandHandlerTests()
        {
            _handler = new UpdateProfileCommandHandler(_userRepository.Object);
            _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        }

        [Fact]
        public async Task Handle_NewUsernameAndAvatar_UpdatesAndSaves()
        {
            _userRepository.Setup(r => r.ExistsByUsernameAsync("alice2", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var (response, error) = await _handler.Handle(new UpdateProfileCommand(1, "alice2", "/uploads/a.png"), CancellationToken.None);

            error.Should().BeNull();
            response!.Username.Should().Be("alice2");
            response.AvatarUrl.Should().Be("/uploads/a.png");
            _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UnchangedUsername_DoesNotCheckUniqueness()
        {
            var (response, error) = await _handler.Handle(new UpdateProfileCommand(1, "alice", null), CancellationToken.None);

            error.Should().BeNull();
            response!.Username.Should().Be("alice");
            _userRepository.Verify(r => r.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UsernameTaken_ReturnsErrorWithoutSaving()
        {
            _userRepository.Setup(r => r.ExistsByUsernameAsync("bob", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var (response, error) = await _handler.Handle(new UpdateProfileCommand(1, "bob", null), CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(UpdateProfileError.UsernameTaken);
            _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyUsername_ReturnsErrorWithoutLookingUpUser()
        {
            var (response, error) = await _handler.Handle(new UpdateProfileCommand(1, "   ", null), CancellationToken.None);

            response.Should().BeNull();
            error.Should().Be(UpdateProfileError.EmptyUsername);
            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
