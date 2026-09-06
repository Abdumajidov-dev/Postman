namespace Pochtachi.Application.Switches;

public record SwitchOptionDto(Guid Id, string Name, int Order);

public record SwitchDimensionDto(Guid Id, Guid WorkspaceId, string Name, Guid? ActiveOptionId, List<SwitchOptionDto> Options);

public record CreateSwitchDimensionRequest(Guid WorkspaceId, string Name, List<string> OptionNames);

public record SetActiveOptionRequest(Guid OptionId);

public record VariableDto(Guid Id, string Key, bool IsSecret, Dictionary<string, string> ValuesByOptionId);

public record CreateVariableRequest(string Key, bool IsSecret);

/// <summary>OptionId=null -> switch tanlanmaganda ishlatiladigan default qiymat.</summary>
public record SetVariableValueRequest(Guid? OptionId, string Value);
