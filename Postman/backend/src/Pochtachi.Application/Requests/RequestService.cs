using System.Text.Json;
using Pochtachi.Application.Common;
using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;
using Pochtachi.Domain.Enums;
using RequestEntity = Pochtachi.Domain.Entities.Request;

namespace Pochtachi.Application.Requests;

public class RequestService(IUnitOfWork uow) : IRequestService
{
    public async Task<List<RequestDto>> ListByCollectionAsync(Guid collectionId, CancellationToken ct = default)
    {
        var items = await uow.Repository<RequestEntity>().ListAsync(r => r.CollectionId == collectionId, ct);
        var result = new List<RequestDto>();
        foreach (var item in items) result.Add(await ToDtoAsync(item, ct));
        return result;
    }

    public async Task<RequestDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var item = await uow.Repository<RequestEntity>().GetByIdAsync(id, ct);
        return item is null ? null : await ToDtoAsync(item, ct);
    }

    public async Task<RequestDto> CreateAsync(CreateRequestRequest request, CancellationToken ct = default)
    {
        var entity = new RequestEntity
        {
            CollectionId = request.CollectionId,
            FolderId = request.FolderId,
            Name = request.Name,
            Method = request.Method,
            Url = request.Url,
        };
        await uow.Repository<RequestEntity>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(entity, ct);
    }

    public async Task<RequestDto?> UpdateAsync(Guid id, UpdateRequestRequest request, CancellationToken ct = default)
    {
        var repo = uow.Repository<RequestEntity>();
        var entity = await repo.GetByIdAsync(id, ct);
        if (entity is null) return null;

        entity.Name = request.Name;
        entity.Method = request.Method;
        entity.Url = request.Url;
        entity.HeadersJson = JsonSerializer.Serialize(request.Headers);
        entity.QueryParamsJson = JsonSerializer.Serialize(request.QueryParams);
        entity.BodyMode = request.BodyMode;
        entity.BodyJson = request.Body;
        entity.PreRequestScript = request.PreRequestScript;
        entity.TestScript = request.TestScript;
        entity.Order = request.Order;

        entity.AuthConfigId = await UpsertAuthAsync(entity.AuthConfigId, request.Auth, ct);

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(entity, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var repo = uow.Repository<RequestEntity>();
        var entity = await repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    private async Task<Guid?> UpsertAuthAsync(Guid? existingAuthConfigId, AuthDto? auth, CancellationToken ct)
    {
        if (auth is null || auth.Type == "NoAuth")
            return null;

        var authRepo = uow.Repository<AuthConfig>();
        var type = Enum.Parse<AuthType>(auth.Type);
        var configJson = JsonSerializer.Serialize(auth.Values);

        if (existingAuthConfigId is Guid id)
        {
            var existing = await authRepo.GetByIdAsync(id, ct);
            if (existing is not null)
            {
                existing.Type = type;
                existing.ConfigJson = configJson;
                authRepo.Update(existing);
                return existing.Id;
            }
        }

        var created = new AuthConfig { Type = type, ConfigJson = configJson };
        await authRepo.AddAsync(created, ct);
        return created.Id;
    }

    private async Task<RequestDto> ToDtoAsync(RequestEntity r, CancellationToken ct)
    {
        AuthDto? authDto = null;
        if (r.AuthConfigId is Guid authId)
        {
            var auth = await uow.Repository<AuthConfig>().GetByIdAsync(authId, ct);
            if (auth is not null)
            {
                var values = JsonSerializer.Deserialize<Dictionary<string, string>>(auth.ConfigJson) ?? [];
                authDto = new AuthDto(auth.Type.ToString(), values);
            }
        }

        return new RequestDto(
            r.Id,
            r.CollectionId,
            r.FolderId,
            r.Name,
            r.Method,
            r.Url,
            JsonSerializer.Deserialize<List<KeyValueDto>>(r.HeadersJson) ?? [],
            JsonSerializer.Deserialize<List<KeyValueDto>>(r.QueryParamsJson) ?? [],
            r.BodyMode,
            r.BodyJson,
            authDto,
            r.PreRequestScript,
            r.TestScript,
            r.Order);
    }
}
