using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class ApiEnvironment : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
