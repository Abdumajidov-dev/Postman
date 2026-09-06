using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.Folders;

public class FolderService(IUnitOfWork uow) : IFolderService
{
    public async Task<List<FolderDto>> ListByCollectionAsync(Guid collectionId, CancellationToken ct = default)
    {
        var items = await uow.Repository<Folder>().ListAsync(f => f.CollectionId == collectionId, ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<FolderDto> CreateAsync(CreateFolderRequest request, CancellationToken ct = default)
    {
        var folder = new Folder
        {
            CollectionId = request.CollectionId,
            ParentFolderId = request.ParentFolderId,
            Name = request.Name,
        };
        await uow.Repository<Folder>().AddAsync(folder, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(folder);
    }

    public async Task<FolderDto?> UpdateAsync(Guid id, UpdateFolderRequest request, CancellationToken ct = default)
    {
        var repo = uow.Repository<Folder>();
        var folder = await repo.GetByIdAsync(id, ct);
        if (folder is null) return null;

        folder.Name = request.Name;
        folder.Order = request.Order;
        repo.Update(folder);
        await uow.SaveChangesAsync(ct);
        return ToDto(folder);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var repo = uow.Repository<Folder>();
        var folder = await repo.GetByIdAsync(id, ct);
        if (folder is null) return false;

        repo.Remove(folder);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    private static FolderDto ToDto(Folder f) => new(f.Id, f.CollectionId, f.ParentFolderId, f.Name, f.Order);
}
