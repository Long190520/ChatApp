namespace ChatApp.Api.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastSeenAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<DirectRoomPair> DirectRoomPairLargerUsers { get; set; } = new List<DirectRoomPair>();

    public virtual ICollection<DirectRoomPair> DirectRoomPairSmallerUsers { get; set; } = new List<DirectRoomPair>();

    public virtual ICollection<Friendship> FriendshipAddressees { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendship> FriendshipRequesters { get; set; } = new List<Friendship>();

    public virtual ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();

    public virtual ICollection<MessageReadReceipt> MessageReadReceipts { get; set; } = new List<MessageReadReceipt>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
}
