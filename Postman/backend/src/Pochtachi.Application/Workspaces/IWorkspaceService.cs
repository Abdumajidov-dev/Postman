namespace Pochtachi.Application.Workspaces;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> ListAsync(CancellationToken ct = default);
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, CancellationToken ct = default);

    /// <summary>Foydalanuvchining birinchi workspace'i — bo'lmasa, "Shaxsiy" workspace yaratadi.</summary>
    Task<WorkspaceDto> GetOrCreateDefaultAsync(Guid ownerId, CancellationToken ct = default);
}
