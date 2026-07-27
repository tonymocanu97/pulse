using MediatR;
using Pulse.Application.Auth.DTOs;
using Pulse.Application.Common.Interfaces.Repositories;

namespace Pulse.Application.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler(IUserRepository userRepository)
        : IRequestHandler<UpdateProfileCommand, (UserDto? Response, UpdateProfileError? Error)>
    {
        public async Task<(UserDto? Response, UpdateProfileError? Error)> Handle(UpdateProfileCommand request, CancellationToken ct)
        {
            var username = request.Username.Trim();
            if (string.IsNullOrEmpty(username))
            {
                return (null, UpdateProfileError.EmptyUsername);
            }

            var user = await userRepository.GetByIdAsync(request.UserId, ct);
            if (user is null)
            {
                return (null, UpdateProfileError.UserNotFound);
            }

            if (!string.Equals(user.Username, username, StringComparison.Ordinal)
                && await userRepository.ExistsByUsernameAsync(username, ct))
            {
                return (null, UpdateProfileError.UsernameTaken);
            }

            user.Username = username;
            user.AvatarUrl = request.AvatarUrl;
            await userRepository.SaveChangesAsync(ct);

            var dto = new UserDto(user.Id, user.Username, user.Email, user.AvatarUrl, user.IsOnline, user.LastSeen);
            return (dto, null);
        }
    }
}
