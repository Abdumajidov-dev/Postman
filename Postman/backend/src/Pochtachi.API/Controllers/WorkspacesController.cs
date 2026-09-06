using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Workspaces;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/workspaces")]
public class WorkspacesController(IWorkspaceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault(CancellationToken ct) => Ok(await service.GetOrCreateDefaultAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));
}
