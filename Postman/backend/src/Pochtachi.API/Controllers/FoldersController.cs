using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Folders;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/folders")]
public class FoldersController(IFolderService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByCollection([FromQuery] Guid collectionId, CancellationToken ct) =>
        Ok(await service.ListByCollectionAsync(collectionId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateFolderRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFolderRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
