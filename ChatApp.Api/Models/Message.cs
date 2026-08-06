namespace ChatApp.Api.Models;

public partial class Message
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid SenderId { get; set; }

    public Guid? ReplyToMessageId { get; set; }

    public string? Content { get; set; }

    public string Type { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Message> InverseReplyToMessage { get; set; } = new List<Message>();

    public virtual ICollection<MessageAttachment> MessageAttachments { get; set; } = new List<MessageAttachment>();

    public virtual ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();

    public virtual ICollection<MessageReadReceipt> MessageReadReceipts { get; set; } = new List<MessageReadReceipt>();

    public virtual Message? ReplyToMessage { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
