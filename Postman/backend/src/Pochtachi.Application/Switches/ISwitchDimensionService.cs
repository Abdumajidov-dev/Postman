namespace Pochtachi.Application.Switches;

public interface ISwitchDimensionService
{
    Task<List<SwitchDimensionDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default);
    Task<SwitchDimensionDto> CreateAsync(CreateSwitchDimensionRequest request, CancellationToken ct = default);
    Task<SwitchDimensionDto?> SetActiveOptionAsync(Guid dimensionId, SetActiveOptionRequest request, CancellationToken ct = default);
    Task<SwitchDimensionDto?> AddOptionAsync(Guid dimensionId, string name, CancellationToken ct = default);
}
