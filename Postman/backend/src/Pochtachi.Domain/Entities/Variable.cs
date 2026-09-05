using Pochtachi.Domain.Common;
using Pochtachi.Domain.Enums;

namespace Pochtachi.Domain.Entities;

/// <summary>
/// O'zgaruvchi o'zi qiymat saqlamaydi — qiymatlari <see cref="VariableValue"/> orqali,
/// har bir aktiv SwitchOption kombinatsiyasi uchun alohida beriladi. Switch o'zgarsa,
/// shu o'zgaruvchiga bog'liq HAMMA joyda (barcha request/collection) natija avtomatik yangilanadi.
/// </summary>
public class Variable : BaseEntity
{
    public VariableScope Scope { get; set; }
    public Guid? OwnerId { get; set; }

    public string Key { get; set; } = string.Empty;
    public bool IsSecret { get; set; }

    public ICollection<VariableValue> Values { get; set; } = new List<VariableValue>();
}
