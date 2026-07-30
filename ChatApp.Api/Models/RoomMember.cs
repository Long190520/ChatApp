using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public partial class RoomMember
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = null!;

    public DateTime JoinedAt { get; set; }

    public DateTime? LastReadAt { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
