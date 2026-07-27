using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Application.Auth.DTOs;
using Pulse.Application.Users.Commands.UpdateProfile;
using Pulse.Application.Users.DTOs;
using Pulse.Application.Users.Queries.SearchUsers;

namespace Pulse.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> Search(
            [FromQuery] string query, [FromQuery] int take = 20, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(Array.Empty<UserDto>());
            }

            var users = await mediator.Send(new SearchUsersQuery(query, CurrentUserId, take), ct);
            return Ok(users);
        }

        [HttpPatch("me")]
        public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(
                new UpdateProfileCommand(CurrentUserId, request.Username, request.AvatarUrl), ct);

            return error switch
            {
                UpdateProfileError.EmptyUsername => BadRequest("Username cannot be empty."),
                UpdateProfileError.UsernameTaken => Conflict("Username is already taken."),
                UpdateProfileError.UserNotFound => NotFound(),
                _ => Ok(response)
            };
        }
    }
}
