using Pochtachi.Domain.Enums;

namespace Pochtachi.Domain.Entities;

public class WorkspaceMember
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
}
