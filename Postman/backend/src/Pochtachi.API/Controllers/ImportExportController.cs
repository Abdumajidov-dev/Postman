using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.ImportExport;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/collections")]
public class ImportExportController(IPostmanImportExportService service) : ControllerBase
{
    [HttpPost("import")]
    public async Task<IActionResult> Import([FromQuery] Guid workspaceId, CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(ct);
        var result = await service.ImportAsync(workspaceId, json, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var json = await service.ExportAsync(id, ct);
        return Content(json, "application/json");
    }
}
