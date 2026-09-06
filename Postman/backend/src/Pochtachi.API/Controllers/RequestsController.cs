using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Requests;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController(IRequestService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByCollection([FromQuery] Guid collectionId, CancellationToken ct) =>
        Ok(await service.ListByCollectionAsync(collectionId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await service.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequestRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRequestRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
