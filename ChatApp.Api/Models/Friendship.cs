using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public partial class Friendship
{
    public Guid Id { get; set; }

    public Guid RequesterId { get; set; }

    public Guid AddresseeId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Addressee { get; set; } = null!;

    public virtual User Requester { get; set; } = null!;
}
