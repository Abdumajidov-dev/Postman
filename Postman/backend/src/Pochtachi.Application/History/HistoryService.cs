using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.History;

public class HistoryService(IUnitOfWork uow) : IHistoryService
{
    public async Task<List<HistoryEntryDto>> ListByWorkspaceAsync(Guid workspaceId, int limit = 50, CancellationToken ct = default)
    {
        var items = await uow.Repository<RequestHistory>().ListAsync(h => h.WorkspaceId == workspaceId, ct);
        return items
            .OrderByDescending(h => h.ExecutedAt)
            .Take(limit)
            .Select(ToDto)
            .ToList();
    }

    public async Task<HistoryEntryDto> CreateAsync(CreateHistoryEntryRequest request, CancellationToken ct = default)
    {
        var entry = new RequestHistory
        {
            WorkspaceId = request.WorkspaceId,
            Method = request.Method,
            Url = request.Url,
            Status = request.Status,
            DurationMs = request.DurationMs,
            RequestSnapshotJson = request.RequestSnapshotJson,
            ResponseSnapshotJson = request.ResponseSnapshotJson,
        };
        await uow.Repository<RequestHistory>().AddAsync(entry, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(entry);
    }

    private static HistoryEntryDto ToDto(RequestHistory h) => new(
        h.Id, h.Method, h.Url, h.Status, h.DurationMs, h.ExecutedAt, h.RequestSnapshotJson, h.ResponseSnapshotJson);
}
