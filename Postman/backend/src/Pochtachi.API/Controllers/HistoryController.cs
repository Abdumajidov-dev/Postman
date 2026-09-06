using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.History;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController(IHistoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByWorkspace([FromQuery] Guid workspaceId, [FromQuery] int limit, CancellationToken ct) =>
        Ok(await service.ListByWorkspaceAsync(workspaceId, limit == 0 ? 50 : limit, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateHistoryEntryRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));
}
