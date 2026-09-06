using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Application.Switches;

public class VariableService(IUnitOfWork uow) : IVariableService
{
    public async Task<List<VariableDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var variables = await uow.Repository<Variable>().ListAsync(v => v.WorkspaceId == workspaceId, ct);
        var result = new List<VariableDto>();
        foreach (var variable in variables)
            result.Add(await ToDtoAsync(variable, ct));
        return result;
    }

    public async Task<VariableDto> CreateAsync(Guid workspaceId, CreateVariableRequest request, CancellationToken ct = default)
    {
        var variable = new Variable { WorkspaceId = workspaceId, Key = request.Key, IsSecret = request.IsSecret };
        await uow.Repository<Variable>().AddAsync(variable, ct);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(variable, ct);
    }

    public async Task<VariableDto?> SetValueAsync(Guid variableId, SetVariableValueRequest request, CancellationToken ct = default)
    {
        var variableRepo = uow.Repository<Variable>();
        var variable = await variableRepo.GetByIdAsync(variableId, ct);
        if (variable is null) return null;

        var valueRepo = uow.Repository<VariableValue>();
        var existing = (await valueRepo.ListAsync(v => v.VariableId == variableId && v.SwitchOptionId == request.OptionId, ct))
            .FirstOrDefault();

        if (existing is not null)
        {
            existing.Value = request.Value;
            valueRepo.Update(existing);
        }
        else
        {
            await valueRepo.AddAsync(new VariableValue
            {
                VariableId = variableId,
                SwitchOptionId = request.OptionId,
                Value = request.Value,
            }, ct);
        }

        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(variable, ct);
    }

    public async Task<bool> DeleteAsync(Guid variableId, CancellationToken ct = default)
    {
        var repo = uow.Repository<Variable>();
        var variable = await repo.GetByIdAsync(variableId, ct);
        if (variable is null) return false;

        var values = await uow.Repository<VariableValue>().ListAsync(v => v.VariableId == variableId, ct);
        var valueRepo = uow.Repository<VariableValue>();
        foreach (var value in values) valueRepo.Remove(value);

        repo.Remove(variable);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Dictionary<string, string>> ResolveAllAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var dimensions = await uow.Repository<SwitchDimension>().ListAsync(d => d.WorkspaceId == workspaceId, ct);
        var activeOptionIds = dimensions.Where(d => d.ActiveOptionId.HasValue).Select(d => d.ActiveOptionId!.Value).ToHashSet();

        var variables = await uow.Repository<Variable>().ListAsync(v => v.WorkspaceId == workspaceId, ct);
        var result = new Dictionary<string, string>();

        foreach (var variable in variables)
        {
            var values = await uow.Repository<VariableValue>().ListAsync(v => v.VariableId == variable.Id, ct);

            var resolved = values.FirstOrDefault(v => v.SwitchOptionId.HasValue && activeOptionIds.Contains(v.SwitchOptionId.Value))
                ?? values.FirstOrDefault(v => v.SwitchOptionId is null);

            if (resolved is not null)
                result[variable.Key] = resolved.Value;
        }

        return result;
    }

    private async Task<VariableDto> ToDtoAsync(Variable variable, CancellationToken ct)
    {
        var values = await uow.Repository<VariableValue>().ListAsync(v => v.VariableId == variable.Id, ct);
        var map = values.ToDictionary(v => v.SwitchOptionId?.ToString() ?? "default", v => v.Value);
        return new VariableDto(variable.Id, variable.Key, variable.IsSecret, map);
    }
}
