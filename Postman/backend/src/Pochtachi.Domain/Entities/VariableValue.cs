using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class VariableValue : BaseEntity
{
    public Guid VariableId { get; set; }
    public Variable? Variable { get; set; }

    /// <summary>Null bo'lsa — hech qanday switch tanlanmaganda ishlatiladigan default qiymat.</summary>
    public Guid? SwitchOptionId { get; set; }
    public SwitchOption? SwitchOption { get; set; }

    public string Value { get; set; } = string.Empty;
}
