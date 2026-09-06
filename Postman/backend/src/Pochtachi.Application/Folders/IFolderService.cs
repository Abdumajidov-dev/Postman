namespace Pochtachi.Application.Folders;

public interface IFolderService
{
    Task<List<FolderDto>> ListByCollectionAsync(Guid collectionId, CancellationToken ct = default);
    Task<FolderDto> CreateAsync(CreateFolderRequest request, CancellationToken ct = default);
    Task<FolderDto?> UpdateAsync(Guid id, UpdateFolderRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
