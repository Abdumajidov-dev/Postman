using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

/// <summary>
/// Mustaqil "o'lcham" — masalan "Environment" (Local/Global) yoki "Role" (Admin/User/SuperAdmin).
/// Har bir dimension o'z switch'iga ega, va o'zgaruvchilar shu switch qiymatiga qarab turli natija beradi.
/// </summary>
public class SwitchDimension : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ActiveOptionId { get; set; }
    public SwitchOption? ActiveOption { get; set; }

    public ICollection<SwitchOption> Options { get; set; } = new List<SwitchOption>();
}
