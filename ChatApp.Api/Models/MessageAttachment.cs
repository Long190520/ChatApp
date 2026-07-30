using System;
using System.Collections.Generic;

namespace ChatApp.Api.Models;

public partial class MessageAttachment
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public virtual Message Message { get; set; } = null!;
}
