namespace ChatApp.Api.Models;

public partial class MessageReaction
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public Guid UserId { get; set; }

    public string Emoji { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
