using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pulse.Application.Auth.DTOs;

namespace Pulse.IntegrationTests
{
    public static class TestDataHelper
    {
        public const string Password = "P@ssw0rd!";

        // Mirrors the JsonStringEnumConverter registered in Program.cs - HttpClient's
        // ReadFromJsonAsync doesn't pick up the API's own MVC JsonOptions, so tests need
        // their own copy to deserialize enum properties (e.g. MessageDto.Type) as strings.
        public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public static async Task<AuthResponse> RegisterAsync(HttpClient client)
        {
            var username = $"user{Guid.NewGuid():N}"[..20];
            var email = $"{Guid.NewGuid():N}@example.com";

            var response = await client.PostAsJsonAsync(
                "/api/auth/register", new RegisterRequest(username, email, Password));

            return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        }
    }
}
