using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.Switches;

public class SwitchDimensionService(IUnitOfWork uow) : ISwitchDimensionService
{
    public async Task<List<SwitchDimensionDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var dimensions = await uow.Repository<SwitchDimension>().ListAsync(d => d.WorkspaceId == workspaceId, ct);
        var result = new List<SwitchDimensionDto>();
        foreach (var dimension in dimensions)
            result.Add(await ToDtoAsync(dimension, ct));
        return result;
    }

    public async Task<SwitchDimensionDto> CreateAsync(CreateSwitchDimensionRequest request, CancellationToken ct = default)
    {
        var dimension = new SwitchDimension { WorkspaceId = request.WorkspaceId, Name = request.Name };
        await uow.Repository<SwitchDimension>().AddAsync(dimension, ct);

        var order = 0;
        foreach (var optionName in request.OptionNames)
            await uow.Repository<SwitchOption>().AddAsync(new SwitchOption
            {
                SwitchDimensionId = dimension.Id,
                Name = optionName,
                Order = order++,
            }, ct);

        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(dimension, ct);
    }

    public async Task<SwitchDimensionDto?> SetActiveOptionAsync(Guid dimensionId, SetActiveOptionRequest request, CancellationToken ct = default)
    {
        var repo = uow.Repository<SwitchDimension>();
        var dimension = await repo.GetByIdAsync(dimensionId, ct);
        if (dimension is null) return null;

        dimension.ActiveOptionId = request.OptionId;
        repo.Update(dimension);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(dimension, ct);
    }

    public async Task<SwitchDimensionDto?> AddOptionAsync(Guid dimensionId, string name, CancellationToken ct = default)
    {
        var dimension = await uow.Repository<SwitchDimension>().GetByIdAsync(dimensionId, ct);
        if (dimension is null) return null;

        var existing = await uow.Repository<SwitchOption>().ListAsync(o => o.SwitchDimensionId == dimensionId, ct);
        await uow.Repository<SwitchOption>().AddAsync(new SwitchOption
        {
            SwitchDimensionId = dimensionId,
            Name = name,
            Order = existing.Count,
        }, ct);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(dimension, ct);
    }

    private async Task<SwitchDimensionDto> ToDtoAsync(SwitchDimension dimension, CancellationToken ct)
    {
        var options = await uow.Repository<SwitchOption>().ListAsync(o => o.SwitchDimensionId == dimension.Id, ct);
        var optionDtos = options.OrderBy(o => o.Order).Select(o => new SwitchOptionDto(o.Id, o.Name, o.Order)).ToList();
        return new SwitchDimensionDto(dimension.Id, dimension.WorkspaceId, dimension.Name, dimension.ActiveOptionId, optionDtos);
    }
}
