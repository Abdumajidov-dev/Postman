namespace Pochtachi.Application.History;

public interface IHistoryService
{
    Task<List<HistoryEntryDto>> ListByWorkspaceAsync(Guid workspaceId, int limit = 50, CancellationToken ct = default);
    Task<HistoryEntryDto> CreateAsync(CreateHistoryEntryRequest request, CancellationToken ct = default);
}
