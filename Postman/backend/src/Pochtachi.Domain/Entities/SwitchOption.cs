using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

/// <summary>Bitta dimension ichidagi bitta variant — masalan "Local" yoki "Admin".</summary>
public class SwitchOption : BaseEntity
{
    public Guid SwitchDimensionId { get; set; }
    public SwitchDimension? SwitchDimension { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}
