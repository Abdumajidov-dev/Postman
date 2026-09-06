using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Pochtachi.Application.Workspaces;

namespace Pochtachi.API.Controllers;

[ApiController]
[Route("api/workspaces")]
public class WorkspacesController(IWorkspaceService service) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault(CancellationToken ct) =>
        Ok(await service.GetOrCreateDefaultAsync(CurrentUserId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));
}
