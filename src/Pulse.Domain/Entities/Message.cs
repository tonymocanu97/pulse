using Pulse.Domain.Enums;

namespace Pulse.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; }

        public ICollection<MessageReaction> Reactions { get; set; } = [];
    }
}
