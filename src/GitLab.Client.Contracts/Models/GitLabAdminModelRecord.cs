using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One record of an instance data model, as returned by the Data management admin endpoints
///     (<c>GET /admin/data_management/:model_name</c>,
///     <c>GET /admin/data_management/:model_name/:record_identifier</c>, and their <c>/checksum</c>
///     siblings). Experimental: GitLab may change this area without deprecation.
/// </summary>
public sealed record GitLabAdminModelRecord
{
    /// <summary>
    ///     The record's identifier within <see cref="ModelClass" />. GitLab types this as either a string
    ///     or an integer depending on the model, so it is surfaced as the raw JSON value rather than as an
    ///     invented union type - read <see cref="JsonElement.ValueKind" /> before converting it.
    /// </summary>
    public JsonElement? RecordIdentifier { get; init; }

    public string? ModelClass { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public long? FileSize { get; init; }

    public GitLabAdminModelChecksumInfo? ChecksumInformation { get; init; }
}