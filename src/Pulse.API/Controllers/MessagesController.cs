using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Application.Messages.Commands.SendMessage;
using Pulse.Application.Messages.Commands.ToggleMessageReaction;
using Pulse.Application.Messages.DTOs;
using Pulse.Application.Messages.Queries.GetMessageHistory;

namespace Pulse.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/conversations/{conversationId:int}/messages")]
    public class MessagesController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetHistory(
            int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        {
            var (response, isForbidden) = await mediator.Send(
                new GetMessageHistoryQuery(conversationId, CurrentUserId, page, pageSize), ct);

            return isForbidden ? Forbid() : Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> Send(int conversationId, SendMessageRequest request, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(
                new SendMessageCommand(conversationId, CurrentUserId, request.Content, request.Type), ct);

            return error switch
            {
                SendMessageError.EmptyContent => BadRequest("Message content cannot be empty."),
                SendMessageError.NotParticipant => Forbid(),
                _ => Ok(response)
            };
        }

        [HttpPost("{messageId:int}/reactions")]
        public async Task<ActionResult<IReadOnlyList<MessageReactionDto>>> ToggleReaction(
            int conversationId, int messageId, ToggleReactionRequest request, CancellationToken ct)
        {
            var (response, error) = await mediator.Send(
                new ToggleMessageReactionCommand(messageId, CurrentUserId, request.Emoji), ct);

            return error switch
            {
                ToggleReactionError.EmptyEmoji => BadRequest("Emoji is required."),
                ToggleReactionError.MessageNotFound => NotFound(),
                ToggleReactionError.NotParticipant => Forbid(),
                _ => Ok(response)
            };
        }
    }
}
