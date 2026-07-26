using MediatR;
using Pulse.Application.Messages.DTOs;
using Pulse.Domain.Enums;

namespace Pulse.Application.Messages.Commands.SendMessage
{
    public enum SendMessageError
    {
        EmptyContent,
        NotParticipant
    }

    public record SendMessageCommand(int ConversationId, int SenderId, string Content, MessageType Type)
        : IRequest<(MessageDto? Response, SendMessageError? Error)>;
}
