namespace Pochtachi.Application.Requests;

public interface IRequestService
{
    Task<List<RequestDto>> ListByCollectionAsync(Guid collectionId, CancellationToken ct = default);
    Task<RequestDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<RequestDto> CreateAsync(CreateRequestRequest request, CancellationToken ct = default);
    Task<RequestDto?> UpdateAsync(Guid id, UpdateRequestRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
