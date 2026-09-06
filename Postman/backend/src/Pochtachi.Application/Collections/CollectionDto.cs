namespace Pochtachi.Application.Collections;

public record CollectionDto(Guid Id, Guid WorkspaceId, string Name, string? Description);

public record CreateCollectionRequest(Guid WorkspaceId, string Name, string? Description);

public record UpdateCollectionRequest(string Name, string? Description);
