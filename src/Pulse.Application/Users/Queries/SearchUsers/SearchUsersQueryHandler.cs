using MediatR;
using Pulse.Application.Auth.DTOs;
using Pulse.Application.Common.Interfaces.Repositories;

namespace Pulse.Application.Users.Queries.SearchUsers
{
    public class SearchUsersQueryHandler(IUserRepository userRepository)
        : IRequestHandler<SearchUsersQuery, IReadOnlyList<UserDto>>
    {
        public async Task<IReadOnlyList<UserDto>> Handle(SearchUsersQuery request, CancellationToken ct)
        {
            var users = await userRepository.SearchAsync(request.Query, request.Take + 1, ct);

            return users
                .Where(u => u.Id != request.ExcludeUserId)
                .Take(request.Take)
                .Select(u => new UserDto(u.Id, u.Username, u.Email, u.AvatarUrl, u.IsOnline, u.LastSeen))
                .ToList();
        }
    }
}
