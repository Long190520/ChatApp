using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public partial class UserConnection
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string ConnectionId { get; set; } = null!;

    public DateTime ConnectedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
