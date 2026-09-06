using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Switches;

namespace Pochtachi.API.Controllers;

public record AddSwitchOptionRequest(string Name);

[ApiController]
[Route("api/switch-dimensions")]
public class SwitchDimensionsController(ISwitchDimensionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByWorkspace([FromQuery] Guid workspaceId, CancellationToken ct) =>
        Ok(await service.ListByWorkspaceAsync(workspaceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSwitchDimensionRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));

    [HttpPut("{id:guid}/active-option")]
    public async Task<IActionResult> SetActiveOption(Guid id, SetActiveOptionRequest request, CancellationToken ct)
    {
        var result = await service.SetActiveOptionAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/options")]
    public async Task<IActionResult> AddOption(Guid id, AddSwitchOptionRequest request, CancellationToken ct)
    {
        var result = await service.AddOptionAsync(id, request.Name, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

[ApiController]
[Route("api/variables")]
public class VariablesController(IVariableService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByWorkspace([FromQuery] Guid workspaceId, CancellationToken ct) =>
        Ok(await service.ListByWorkspaceAsync(workspaceId, ct));

    [HttpGet("resolved")]
    public async Task<IActionResult> ResolveAll([FromQuery] Guid workspaceId, CancellationToken ct) =>
        Ok(await service.ResolveAllAsync(workspaceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] Guid workspaceId, CreateVariableRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(workspaceId, request, ct));

    [HttpPut("{id:guid}/value")]
    public async Task<IActionResult> SetValue(Guid id, SetVariableValueRequest request, CancellationToken ct)
    {
        var result = await service.SetValueAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
