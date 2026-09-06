using Pochtachi.Application.Collections;

namespace Pochtachi.Application.ImportExport;

public interface IPostmanImportExportService
{
    /// <summary>Postman Collection v2.1 JSON'ni import qiladi — yangi Collection, ichidagi
    /// Folder/Request'lar bilan birga (nested papkalar saqlanadi).</summary>
    Task<CollectionDto> ImportAsync(Guid workspaceId, string postmanJson, CancellationToken ct = default);

    /// <summary>Collection'ni Postman Collection v2.1 formatida JSON string sifatida eksport qiladi.</summary>
    Task<string> ExportAsync(Guid collectionId, CancellationToken ct = default);
}
