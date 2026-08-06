using System.ComponentModel.DataAnnotations;

namespace ChatApp.Api.Dto
{
    public class SendMessageRequestDto
    {
        public Guid RoomId { get; set; }
        public string? Content { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        [RegularExpression("^(Text|Image|File|System)$", ErrorMessage = "Loại tin nhắn không hợp lệ.")]
        public string MessageType { get; set; } = "Text";
    }

    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public required SenderDto Sender { get; set; }
        public string? Content { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public required string MessageType { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? EditedAt { get; set; }
    }

    public class SenderDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
