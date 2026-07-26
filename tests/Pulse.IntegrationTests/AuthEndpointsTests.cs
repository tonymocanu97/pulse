using System.Net;
using System.Net.Http.Json;
using Pulse.Application.Auth.DTOs;

namespace Pulse.IntegrationTests
{
    [Collection(ApiTestCollection.Name)]
    public class AuthEndpointsTests(ApiWebApplicationFactory factory)
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Register_NewUser_ReturnsOkWithToken()
        {
            var username = $"user{Guid.NewGuid():N}"[..20];
            var email = $"{Guid.NewGuid():N}@example.com";
            var request = new RegisterRequest(username, email, TestDataHelper.Password);

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
            body!.Token.Should().NotBeNullOrEmpty();
            body.User.Username.Should().Be(username);
            body.User.Email.Should().Be(email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var email = $"{Guid.NewGuid():N}@example.com";
            var first = new RegisterRequest($"user{Guid.NewGuid():N}"[..20], email, TestDataHelper.Password);
            await _client.PostAsJsonAsync("/api/auth/register", first);

            var second = new RegisterRequest($"user{Guid.NewGuid():N}"[..20], email, TestDataHelper.Password);
            var response = await _client.PostAsJsonAsync("/api/auth/register", second);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var username = $"user{Guid.NewGuid():N}"[..20];
            var email = $"{Guid.NewGuid():N}@example.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(username, email, TestDataHelper.Password));

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, TestDataHelper.Password));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
            body!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var username = $"user{Guid.NewGuid():N}"[..20];
            var email = $"{Guid.NewGuid():N}@example.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(username, email, TestDataHelper.Password));

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword!"));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest($"{Guid.NewGuid():N}@example.com", TestDataHelper.Password));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
