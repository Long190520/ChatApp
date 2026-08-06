namespace ChatApp.Api.Models;

public partial class Room
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string Type { get; set; } = null!;

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<DirectRoomPair> DirectRoomPairs { get; set; } = new List<DirectRoomPair>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
}
