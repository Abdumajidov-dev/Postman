namespace Pochtachi.Application.History;

public record HistoryEntryDto(Guid Id, string Method, string Url, int? Status, long DurationMs, DateTime ExecutedAt, string RequestSnapshotJson, string ResponseSnapshotJson);

public record CreateHistoryEntryRequest(Guid WorkspaceId, string Method, string Url, int? Status, long DurationMs, string RequestSnapshotJson, string ResponseSnapshotJson);
