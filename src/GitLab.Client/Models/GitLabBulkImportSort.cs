using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The creation-time sort direction shared by the migration listings
///     (<c>/bulk_imports</c>, <c>/bulk_imports/entities</c>, <c>/offline_exports</c>). GitLab defaults to
///     <see cref="Desc" /> - newest first - when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBulkImportSort>))]
public enum GitLabBulkImportSort
{
    [JsonStringEnumMemberName("asc")] Asc,

    [JsonStringEnumMemberName("desc")] Desc
}