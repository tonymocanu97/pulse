using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pulse.Application.Auth.Commands.Login;
using Pulse.Application.Auth.Commands.Register;
using Pulse.Application.Auth.DTOs;

namespace Pulse.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(
                new RegisterCommand(request.Username, request.Email, request.Password), ct);

            return error is not null ? Conflict(error) : Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            var response = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
            return response is null ? Unauthorized("Invalid email or password.") : Ok(response);
        }
    }
}
