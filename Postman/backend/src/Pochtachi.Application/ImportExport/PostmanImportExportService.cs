using System.Text.Json;
using System.Text.Json.Nodes;
using Pochtachi.Application.Collections;
using Pochtachi.Application.Common;
using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;
using Pochtachi.Domain.Enums;
using RequestEntity = Pochtachi.Domain.Entities.Request;

namespace Pochtachi.Application.ImportExport;

/// <summary>Postman Collection v2.1 formatiga import/eksport. Nested papkalar, header/query/body
/// va asosiy auth turlari (Bearer/Basic/ApiKey/OAuth2/Digest) qo'llab-quvvatlanadi.</summary>
public class PostmanImportExportService(IUnitOfWork uow) : IPostmanImportExportService
{
    public async Task<CollectionDto> ImportAsync(Guid workspaceId, string postmanJson, CancellationToken ct = default)
    {
        var root = JsonNode.Parse(postmanJson)?.AsObject()
            ?? throw new InvalidOperationException("Yaroqsiz JSON.");

        var name = root["info"]?["name"]?.GetValue<string>() ?? "Imported collection";

        var collection = new Collection { WorkspaceId = workspaceId, Name = name };
        await uow.Repository<Collection>().AddAsync(collection, ct);
        await uow.SaveChangesAsync(ct);

        var items = root["item"]?.AsArray();
        if (items is not null)
            foreach (var item in items)
                if (item is not null)
                    await ImportItemAsync(collection.Id, null, item.AsObject(), ct);

        return new CollectionDto(collection.Id, collection.WorkspaceId, collection.Name, collection.Description, null);
    }

    private async Task ImportItemAsync(Guid collectionId, Guid? parentFolderId, JsonObject item, CancellationToken ct)
    {
        var name = item["name"]?.GetValue<string>() ?? "Untitled";
        var childItems = item["item"]?.AsArray();

        if (childItems is not null)
        {
            var folder = new Folder { CollectionId = collectionId, ParentFolderId = parentFolderId, Name = name };
            await uow.Repository<Folder>().AddAsync(folder, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var child in childItems)
                if (child is not null)
                    await ImportItemAsync(collectionId, folder.Id, child.AsObject(), ct);
            return;
        }

        var requestNode = item["request"]?.AsObject();
        if (requestNode is null) return;

        var method = requestNode["method"]?.GetValue<string>() ?? "GET";
        var (url, query) = ParseUrl(requestNode["url"]);
        var headers = ParseKeyValueArray(requestNode["header"]?.AsArray());
        var (bodyMode, body) = ParseBody(requestNode["body"]?.AsObject());
        var auth = ParseAuth(requestNode["auth"]?.AsObject());

        var request = new RequestEntity
        {
            CollectionId = collectionId,
            FolderId = parentFolderId,
            Name = name,
            Method = method.ToUpperInvariant(),
            Url = url,
            HeadersJson = JsonSerializer.Serialize(headers),
            QueryParamsJson = JsonSerializer.Serialize(query),
            BodyMode = bodyMode,
            BodyJson = body,
        };

        if (auth is not null)
        {
            var authConfig = new AuthConfig { Type = auth.Value.Type, ConfigJson = JsonSerializer.Serialize(auth.Value.Values) };
            await uow.Repository<AuthConfig>().AddAsync(authConfig, ct);
            await uow.SaveChangesAsync(ct);
            request.AuthConfigId = authConfig.Id;
        }

        await uow.Repository<RequestEntity>().AddAsync(request, ct);
        await uow.SaveChangesAsync(ct);
    }

    private static (string Url, List<KeyValueDto> Query) ParseUrl(JsonNode? urlNode)
    {
        if (urlNode is null) return ("", []);

        string? raw = urlNode is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
        if (raw is not null) return (StripQuery(raw), []);

        var obj = urlNode.AsObject();
        var rawUrl = obj["raw"]?.GetValue<string>() ?? "";
        var query = ParseKeyValueArray(obj["query"]?.AsArray());
        return (StripQuery(rawUrl), query);
    }

    private static string StripQuery(string url)
    {
        var qIndex = url.IndexOf('?');
        return qIndex >= 0 ? url[..qIndex] : url;
    }

    private static List<KeyValueDto> ParseKeyValueArray(JsonArray? arr)
    {
        var list = new List<KeyValueDto>();
        if (arr is null) return list;

        foreach (var node in arr)
        {
            if (node is null) continue;
            var obj = node.AsObject();
            var key = obj["key"]?.GetValue<string>() ?? "";
            var value = obj["value"]?.GetValue<string>() ?? "";
            var disabled = obj["disabled"]?.GetValue<bool>() ?? false;
            list.Add(new KeyValueDto(key, value, !disabled));
        }
        return list;
    }

    private static (string Mode, string? Body) ParseBody(JsonObject? bodyObj)
    {
        if (bodyObj is null) return ("none", null);
        var mode = bodyObj["mode"]?.GetValue<string>() ?? "none";

        return mode switch
        {
            "raw" => ("raw-json", bodyObj["raw"]?.GetValue<string>()),
            "urlencoded" => ("x-www-form-urlencoded", JsonSerializer.Serialize(ParseKeyValueArray(bodyObj["urlencoded"]?.AsArray()))),
            "formdata" => ("form-data", JsonSerializer.Serialize(ParseKeyValueArray(bodyObj["formdata"]?.AsArray()))),
            _ => ("none", null),
        };
    }

    private static (AuthType Type, Dictionary<string, string> Values)? ParseAuth(JsonObject? authObj)
    {
        if (authObj is null) return null;
        var type = authObj["type"]?.GetValue<string>() ?? "noauth";

        Dictionary<string, string> ExtractParams(string arrKey)
        {
            var dict = new Dictionary<string, string>();
            var arr = authObj[arrKey]?.AsArray();
            if (arr is null) return dict;
            foreach (var node in arr)
            {
                if (node is null) continue;
                var obj = node.AsObject();
                var key = obj["key"]?.GetValue<string>();
                var value = obj["value"]?.GetValue<string>() ?? "";
                if (key is not null) dict[key] = value;
            }
            return dict;
        }

        return type switch
        {
            "bearer" => (AuthType.Bearer, ExtractParams("bearer")),
            "basic" => (AuthType.Basic, ExtractParams("basic")),
            "apikey" => (AuthType.ApiKey, ExtractParams("apikey")),
            "oauth2" => (AuthType.OAuth2, ExtractParams("oauth2")),
            "digest" => (AuthType.Digest, ExtractParams("digest")),
            _ => null,
        };
    }

    public async Task<string> ExportAsync(Guid collectionId, CancellationToken ct = default)
    {
        var collection = await uow.Repository<Collection>().GetByIdAsync(collectionId, ct)
            ?? throw new InvalidOperationException("Collection topilmadi.");

        var folders = await uow.Repository<Folder>().ListAsync(f => f.CollectionId == collectionId, ct);
        var requests = await uow.Repository<RequestEntity>().ListAsync(r => r.CollectionId == collectionId, ct);

        var root = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["name"] = collection.Name,
                ["schema"] = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
            },
            ["item"] = await BuildItemsAsync(null, folders, requests, ct),
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<JsonArray> BuildItemsAsync(Guid? parentFolderId, List<Folder> allFolders, List<RequestEntity> allRequests, CancellationToken ct)
    {
        var array = new JsonArray();

        foreach (var folder in allFolders.Where(f => f.ParentFolderId == parentFolderId).OrderBy(f => f.Order))
        {
            array.Add(new JsonObject
            {
                ["name"] = folder.Name,
                ["item"] = await BuildItemsAsync(folder.Id, allFolders, allRequests, ct),
            });
        }

        foreach (var request in allRequests.Where(r => r.FolderId == parentFolderId).OrderBy(r => r.Order))
            array.Add(await BuildRequestItemAsync(request, ct));

        return array;
    }

    private async Task<JsonObject> BuildRequestItemAsync(RequestEntity request, CancellationToken ct)
    {
        var headers = JsonSerializer.Deserialize<List<KeyValueDto>>(request.HeadersJson) ?? [];
        var query = JsonSerializer.Deserialize<List<KeyValueDto>>(request.QueryParamsJson) ?? [];

        var urlObj = new JsonObject
        {
            ["raw"] = query.Count > 0
                ? $"{request.Url}?{string.Join('&', query.Select(q => $"{q.Key}={q.Value}"))}"
                : request.Url,
            ["query"] = new JsonArray(query.Select(q => (JsonNode)new JsonObject
            {
                ["key"] = q.Key,
                ["value"] = q.Value,
                ["disabled"] = !q.Enabled,
            }).ToArray()),
        };

        var requestObj = new JsonObject
        {
            ["method"] = request.Method,
            ["header"] = new JsonArray(headers.Select(h => (JsonNode)new JsonObject
            {
                ["key"] = h.Key,
                ["value"] = h.Value,
                ["disabled"] = !h.Enabled,
            }).ToArray()),
            ["url"] = urlObj,
        };

        if (request.BodyMode != "none")
            requestObj["body"] = BuildBody(request.BodyMode, request.BodyJson);

        if (request.AuthConfigId is Guid authId)
        {
            var auth = await uow.Repository<AuthConfig>().GetByIdAsync(authId, ct);
            if (auth is not null) requestObj["auth"] = BuildAuth(auth);
        }

        return new JsonObject { ["name"] = request.Name, ["request"] = requestObj };
    }

    private static JsonObject BuildBody(string mode, string? body) => mode switch
    {
        "raw-json" or "raw-text" => new JsonObject { ["mode"] = "raw", ["raw"] = body ?? "" },
        "x-www-form-urlencoded" => new JsonObject { ["mode"] = "urlencoded", ["urlencoded"] = ToKeyValueArray(body) },
        "form-data" => new JsonObject { ["mode"] = "formdata", ["formdata"] = ToKeyValueArray(body) },
        _ => new JsonObject { ["mode"] = "raw", ["raw"] = "" },
    };

    private static JsonArray ToKeyValueArray(string? json)
    {
        var list = string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<KeyValueDto>>(json) ?? [];
        return new JsonArray(list.Select(kv => (JsonNode)new JsonObject
        {
            ["key"] = kv.Key,
            ["value"] = kv.Value,
            ["disabled"] = !kv.Enabled,
        }).ToArray());
    }

    private static JsonObject BuildAuth(AuthConfig auth)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(auth.ConfigJson) ?? [];
        var typeStr = auth.Type.ToString().ToLowerInvariant();
        var arr = new JsonArray(values.Select(kv => (JsonNode)new JsonObject { ["key"] = kv.Key, ["value"] = kv.Value }).ToArray());
        return new JsonObject { ["type"] = typeStr, [typeStr] = arr };
    }
}
