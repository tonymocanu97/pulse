using MediatR;
using Pulse.Application.Auth.DTOs;

namespace Pulse.Application.Users.Queries.SearchUsers
{
    public record SearchUsersQuery(string Query, int ExcludeUserId, int Take) : IRequest<IReadOnlyList<UserDto>>;
}
