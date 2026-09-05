using Pochtachi.Domain.Common;
using Pochtachi.Domain.Enums;

namespace Pochtachi.Domain.Entities;

public class AuthConfig : BaseEntity
{
    public AuthType Type { get; set; } = AuthType.NoAuth;

    /// <summary>Turga qarab har xil shakl (token, key/value, oauth params) — JSON sifatida saqlanadi.</summary>
    public string ConfigJson { get; set; } = "{}";
}
