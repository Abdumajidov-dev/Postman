using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class RequestHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string RequestSnapshotJson { get; set; } = "{}";
    public string ResponseSnapshotJson { get; set; } = "{}";
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
