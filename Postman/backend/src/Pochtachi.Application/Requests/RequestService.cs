using System.Text.Json;
using Pochtachi.Application.Common;
using Pochtachi.Domain.Common;
using RequestEntity = Pochtachi.Domain.Entities.Request;

namespace Pochtachi.Application.Requests;

public class RequestService(IUnitOfWork uow) : IRequestService
{
    public async Task<List<RequestDto>> ListByCollectionAsync(Guid collectionId, CancellationToken ct = default)
    {
        var items = await uow.Repository<RequestEntity>().ListAsync(r => r.CollectionId == collectionId, ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<RequestDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var item = await uow.Repository<RequestEntity>().GetByIdAsync(id, ct);
        return item is null ? null : ToDto(item);
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
        return ToDto(entity);
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
        entity.BodyJson = request.Body;
        entity.PreRequestScript = request.PreRequestScript;
        entity.TestScript = request.TestScript;
        entity.Order = request.Order;

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);
        return ToDto(entity);
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

    private static RequestDto ToDto(RequestEntity r) => new(
        r.Id,
        r.CollectionId,
        r.FolderId,
        r.Name,
        r.Method,
        r.Url,
        JsonSerializer.Deserialize<List<KeyValueDto>>(r.HeadersJson) ?? [],
        JsonSerializer.Deserialize<List<KeyValueDto>>(r.QueryParamsJson) ?? [],
        r.BodyJson,
        r.PreRequestScript,
        r.TestScript,
        r.Order);
}
