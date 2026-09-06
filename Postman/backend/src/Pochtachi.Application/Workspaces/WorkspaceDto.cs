namespace Pochtachi.Application.Workspaces;

public record WorkspaceDto(Guid Id, string Name, DateTime CreatedAt);

public record CreateWorkspaceRequest(string Name);
