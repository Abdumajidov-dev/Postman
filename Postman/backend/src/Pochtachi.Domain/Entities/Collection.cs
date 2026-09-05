using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class Collection : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? AuthConfigId { get; set; }
    public AuthConfig? AuthConfig { get; set; }

    public ICollection<Folder> Folders { get; set; } = new List<Folder>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
