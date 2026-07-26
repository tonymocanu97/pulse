namespace Pulse.Application.Auth.DTOs
{
    public record RegisterRequest(string Username, string Email, string Password);

    public record LoginRequest(string Email, string Password);

    public record UserDto(int Id, string Username, string Email, string? AvatarUrl, bool IsOnline, DateTime? LastSeen);

    public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
}
