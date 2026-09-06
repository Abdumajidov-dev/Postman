namespace Pochtachi.Application.Switches;

public interface IVariableService
{
    Task<List<VariableDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default);
    Task<VariableDto> CreateAsync(Guid workspaceId, CreateVariableRequest request, CancellationToken ct = default);
    Task<VariableDto?> SetValueAsync(Guid variableId, SetVariableValueRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid variableId, CancellationToken ct = default);

    /// <summary>Har bir dimension'ning joriy aktiv switch holatiga qarab, workspace'dagi
    /// barcha o'zgaruvchini bitta key->qiymat map'ga resolve qiladi.</summary>
    Task<Dictionary<string, string>> ResolveAllAsync(Guid workspaceId, CancellationToken ct = default);
}
