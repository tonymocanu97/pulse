namespace Pulse.Domain.Entities
{
    public class Participant
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Watermark-style read receipt: messages in this conversation sent at or
        // before LastReadAt are considered read by this participant.
        public DateTime? LastReadAt { get; set; }
    }
}
