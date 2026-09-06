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

    public async Task<WorkspaceDto> GetOrCreateDefaultAsync(Guid ownerId, CancellationToken ct = default)
    {
        var owned = await uow.Repository<Workspace>().ListAsync(w => w.OwnerId == ownerId, ct);
        if (owned.Count > 0) return ToDto(owned[0]);

        var workspace = new Workspace { Name = "Shaxsiy workspace", OwnerId = ownerId };
        await uow.Repository<Workspace>().AddAsync(workspace, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(workspace);
    }

    private static WorkspaceDto ToDto(Workspace w) => new(w.Id, w.Name, w.CreatedAt);
}
