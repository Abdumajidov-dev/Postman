namespace Pochtachi.Application.Collections;

public interface ICollectionService
{
    Task<List<CollectionDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollectionDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<CollectionDto> CreateAsync(CreateCollectionRequest request, CancellationToken ct = default);
    Task<CollectionDto?> UpdateAsync(Guid id, UpdateCollectionRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
