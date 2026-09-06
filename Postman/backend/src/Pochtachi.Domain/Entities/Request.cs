using Pochtachi.Domain.Common;

namespace Pochtachi.Domain.Entities;

public class Request : BaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection? Collection { get; set; }

    public Guid? FolderId { get; set; }
    public Folder? Folder { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;

    /// <summary>Header/query-param/body — barchasi JSON sifatida saqlanadi (moslashuvchan schema).</summary>
    public string HeadersJson { get; set; } = "[]";
    public string QueryParamsJson { get; set; } = "[]";

    /// <summary>none | raw-json | raw-text | x-www-form-urlencoded | form-data</summary>
    public string BodyMode { get; set; } = "none";

    /// <summary>raw-* uchun matn, urlencoded/form-data uchun KeyValueDto[] JSON.</summary>
    public string? BodyJson { get; set; }

    public Guid? AuthConfigId { get; set; }
    public AuthConfig? AuthConfig { get; set; }

    public string? PreRequestScript { get; set; }
    public string? TestScript { get; set; }

    public int Order { get; set; }
}
