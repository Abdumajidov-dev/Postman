using System.Text.Json;
using Pochtachi.Application.Common;
using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;
using Pochtachi.Domain.Enums;

namespace Pochtachi.Application.Collections;

public class CollectionService(IUnitOfWork uow) : ICollectionService
{
    public async Task<List<CollectionDto>> ListByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var items = await uow.Repository<Collection>().ListAsync(c => c.WorkspaceId == workspaceId, ct);
        var result = new List<CollectionDto>();
        foreach (var item in items) result.Add(await ToDtoAsync(item, ct));
        return result;
    }

    public async Task<CollectionDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var item = await uow.Repository<Collection>().GetByIdAsync(id, ct);
        return item is null ? null : await ToDtoAsync(item, ct);
    }

    public async Task<CollectionDto> CreateAsync(CreateCollectionRequest request, CancellationToken ct = default)
    {
        var collection = new Collection
        {
            WorkspaceId = request.WorkspaceId,
            Name = request.Name,
            Description = request.Description,
        };
        await uow.Repository<Collection>().AddAsync(collection, ct);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(collection, ct);
    }

    public async Task<CollectionDto?> UpdateAsync(Guid id, UpdateCollectionRequest request, CancellationToken ct = default)
    {
        var repo = uow.Repository<Collection>();
        var collection = await repo.GetByIdAsync(id, ct);
        if (collection is null) return null;

        collection.Name = request.Name;
        collection.Description = request.Description;
        collection.AuthConfigId = await UpsertAuthAsync(collection.AuthConfigId, request.Auth, ct);

        repo.Update(collection);
        await uow.SaveChangesAsync(ct);
        return await ToDtoAsync(collection, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var repo = uow.Repository<Collection>();
        var collection = await repo.GetByIdAsync(id, ct);
        if (collection is null) return false;

        repo.Remove(collection);
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

    private async Task<CollectionDto> ToDtoAsync(Collection c, CancellationToken ct)
    {
        AuthDto? authDto = null;
        if (c.AuthConfigId is Guid authId)
        {
            var auth = await uow.Repository<AuthConfig>().GetByIdAsync(authId, ct);
            if (auth is not null)
            {
                var values = JsonSerializer.Deserialize<Dictionary<string, string>>(auth.ConfigJson) ?? [];
                authDto = new AuthDto(auth.Type.ToString(), values);
            }
        }

        return new CollectionDto(c.Id, c.WorkspaceId, c.Name, c.Description, authDto);
    }
}
