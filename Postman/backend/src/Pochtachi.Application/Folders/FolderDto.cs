namespace Pochtachi.Application.Folders;

public record FolderDto(Guid Id, Guid CollectionId, Guid? ParentFolderId, string Name, int Order);

public record CreateFolderRequest(Guid CollectionId, Guid? ParentFolderId, string Name);

public record UpdateFolderRequest(string Name, int Order);
