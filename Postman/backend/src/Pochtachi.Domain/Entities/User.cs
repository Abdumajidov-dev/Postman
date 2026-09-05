using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();
}
