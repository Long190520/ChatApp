namespace ChatApp.Api.Models;

public partial class MessageReadReceipt
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public Guid UserId { get; set; }

    public DateTime ReadAt { get; set; }

    public virtual Message Message { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
