namespace Pochtachi.Application.Workspaces;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> ListAsync(CancellationToken ct = default);
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, CancellationToken ct = default);

    /// <summary>Birinchi ishga tushirishda — hech qanday workspace bo'lmasa, "Shaxsiy" workspace yaratadi.</summary>
    Task<WorkspaceDto> GetOrCreateDefaultAsync(CancellationToken ct = default);
}
