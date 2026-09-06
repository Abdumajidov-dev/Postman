using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.Collections;

public class CollectionService(IUnitOfWork uow) : ICollectionService
{
    public async Task<List<CollectionDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var items = await uow.Repository<Collection>().ListAsync(c => c.WorkspaceId == workspaceId, ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<CollectionDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var item = await uow.Repository<Collection>().GetByIdAsync(id, ct);
        return item is null ? null : ToDto(item);
    }

    public async Task<CollectionDto> CreateAsync(CreateCollectionRequest request, CancellationToken ct = default)
    {
        var collection = new Collection
        {
            WorkspaceId = request.WorkspaceId,
            Name = request.Name,
            Description = request.Description,
        };
        await uow.Repository<Collection>().AddAsync(collection, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(collection);
    }

    public async Task<CollectionDto?> UpdateAsync(Guid id, UpdateCollectionRequest request, CancellationToken ct = default)
    {
        var repo = uow.Repository<Collection>();
        var collection = await repo.GetByIdAsync(id, ct);
        if (collection is null) return null;

        collection.Name = request.Name;
        collection.Description = request.Description;
        repo.Update(collection);
        await uow.SaveChangesAsync(ct);
        return ToDto(collection);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var repo = uow.Repository<Collection>();
        var collection = await repo.GetByIdAsync(id, ct);
        if (collection is null) return false;

        repo.Remove(collection);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    private static CollectionDto ToDto(Collection c) => new(c.Id, c.WorkspaceId, c.Name, c.Description);
}
