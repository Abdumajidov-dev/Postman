using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
    public ICollection<ApiEnvironment> Environments { get; set; } = new List<ApiEnvironment>();
    public ICollection<SwitchDimension> SwitchDimensions { get; set; } = new List<SwitchDimension>();
    public ICollection<Variable> Variables { get; set; } = new List<Variable>();
}
