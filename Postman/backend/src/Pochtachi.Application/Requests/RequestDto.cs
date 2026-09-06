using Pochtachi.Application.Common;

namespace Pochtachi.Application.Requests;

public record RequestDto(
    Guid Id,
    Guid CollectionId,
    Guid? FolderId,
    string Name,
    string Method,
    string Url,
    List<KeyValueDto> Headers,
    List<KeyValueDto> QueryParams,
    string BodyMode,
    string? Body,
    AuthDto? Auth,
    string? PreRequestScript,
    string? TestScript,
    int Order);

public record CreateRequestRequest(Guid CollectionId, Guid? FolderId, string Name, string Method, string Url);

public record UpdateRequestRequest(
    string Name,
    string Method,
    string Url,
    List<KeyValueDto> Headers,
    List<KeyValueDto> QueryParams,
    string BodyMode,
    string? Body,
    AuthDto? Auth,
    string? PreRequestScript,
    string? TestScript,
    int Order);
