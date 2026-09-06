using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Collections;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/collections")]
public class CollectionsController(ICollectionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByWorkspace([FromQuery] Guid workspaceId, CancellationToken ct) =>
        Ok(await service.ListByWorkspaceAsync(workspaceId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await service.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCollectionRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCollectionRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
