using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Application.Auth.DTOs;
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
    }
}
