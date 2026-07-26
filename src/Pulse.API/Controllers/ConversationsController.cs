using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Application.Conversations.Commands.CreateConversation;
using Pulse.Application.Conversations.DTOs;
using Pulse.Application.Conversations.Queries.GetConversationById;
using Pulse.Application.Conversations.Queries.GetUserConversations;

namespace Pulse.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetAll(CancellationToken ct)
        {
            var conversations = await mediator.Send(new GetUserConversationsQuery(CurrentUserId), ct);
            return Ok(conversations);
        }

        [HttpPost]
        public async Task<ActionResult<ConversationDto>> Create(CreateConversationRequest request, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(
                new CreateConversationCommand(CurrentUserId, request.ParticipantUserIds, request.Name, request.IsGroup), ct);

            return error is not null ? Conflict(error) : Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConversationDto>> GetById(int id, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(new GetConversationByIdQuery(id, CurrentUserId), ct);

            return error switch
            {
                ConversationAccessError.NotFound => NotFound(),
                ConversationAccessError.Forbidden => Forbid(),
                _ => Ok(response)
            };
        }
    }
}
