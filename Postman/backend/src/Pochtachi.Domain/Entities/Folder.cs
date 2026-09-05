using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class Folder : BaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection? Collection { get; set; }

    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    public ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
