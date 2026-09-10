using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One advanced-search (Elasticsearch/Zoekt) background migration, as returned by
///     <c>GET /admin/search/migrations/:migration_id</c>.
/// </summary>
public sealed record GitLabSearchMigration
{
    /// <summary>The migration's version number - also its identifier when looking one up by <c>migration_id</c>.</summary>
    public required long Version { get; init; }

    public string? Name { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public bool? Completed { get; init; }

    /// <summary>Whether this migration has been dropped from newer GitLab versions.</summary>
    public bool? Obsolete { get; init; }

    /// <summary>
    ///     Migration-specific progress data (for example a background job id). The spec types this as a bare
    ///     <c>object</c> whose shape is private to each migration, so it is kept as a raw
    ///     <see cref="JsonElement" /> rather than forced into an invented shape.
    /// </summary>
    public JsonElement? MigrationState { get; init; }
}