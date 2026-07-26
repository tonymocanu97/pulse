using Microsoft.Extensions.Logging.Abstractions;
using Pulse.Application.Auth.Commands.Register;
using Pulse.Application.Common.Interfaces;
using Pulse.Application.Common.Interfaces.Repositories;
using Pulse.Domain.Entities;

namespace Pulse.UnitTests.Auth
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<IJwtTokenService> _jwtTokenService = new();
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _handler = new RegisterCommandHandler(
                _userRepository.Object,
                _passwordHasher.Object,
                _jwtTokenService.Object,
                NullLogger<RegisterCommandHandler>.Instance);
        }

        [Fact]
        public async Task Handle_WithNewEmailAndUsername_CreatesUserAndReturnsAuthResponse()
        {
            _userRepository.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepository.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _passwordHasher.Setup(h => h.Hash("P@ssw0rd!")).Returns("hashed-password");
            _jwtTokenService.Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

            var command = new RegisterCommand("alice", "alice@example.com", "P@ssw0rd!");

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            error.Should().BeNull();
            response.Should().NotBeNull();
            response!.Token.Should().Be("jwt-token");
            response.User.Username.Should().Be("alice");
            response.User.Email.Should().Be("alice@example.com");

            _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "alice" && u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
            _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithAlreadyRegisteredEmail_ReturnsErrorWithoutCreatingUser()
        {
            _userRepository.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new RegisterCommand("alice", "alice@example.com", "P@ssw0rd!");

            var (response, error) = await _handler.Handle(command, CancellationToken.None);

            response.Should().BeNull();
            error.Should().Contain("alice@example.com");

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
