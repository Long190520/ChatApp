using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public partial class DirectRoomPair
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid SmallerUserId { get; set; }

    public Guid LargerUserId { get; set; }

    public virtual User LargerUser { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual User SmallerUser { get; set; } = null!;
}
