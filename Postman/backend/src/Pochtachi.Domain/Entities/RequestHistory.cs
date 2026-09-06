using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class RequestHistory : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    /// <summary>Auth hali ulanmagan bosqichda null bo'lishi mumkin.</summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public int? Status { get; set; }
    public long DurationMs { get; set; }

    public string RequestSnapshotJson { get; set; } = "{}";
    public string ResponseSnapshotJson { get; set; } = "{}";
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
