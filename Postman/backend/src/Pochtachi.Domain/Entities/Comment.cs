using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public enum CommentEntityType
{
    Collection = 0,
    Folder = 1,
    Request = 2
}

public class Comment : BaseEntity
{
    public CommentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Text { get; set; } = string.Empty;
}
