using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.Workspaces;

public class WorkspaceService(IUnitOfWork uow) : IWorkspaceService
{
    public async Task<List<WorkspaceDto>> ListAsync(CancellationToken ct = default)
    {
        var workspaces = await uow.Repository<Workspace>().ListAsync(ct: ct);
        return workspaces.Select(ToDto).ToList();
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, CancellationToken ct = default)
    {
        var workspace = new Workspace { Name = request.Name };
        await uow.Repository<Workspace>().AddAsync(workspace, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(workspace);
    }

    public async Task<WorkspaceDto> GetOrCreateDefaultAsync(CancellationToken ct = default)
    {
        var existing = await uow.Repository<Workspace>().ListAsync(ct: ct);
        if (existing.Count > 0) return ToDto(existing[0]);
        return await CreateAsync(new CreateWorkspaceRequest("Shaxsiy workspace"), ct);
    }

    private static WorkspaceDto ToDto(Workspace w) => new(w.Id, w.Name, w.CreatedAt);
}
