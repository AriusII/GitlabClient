using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One group or project inside a direct-transfer migration
///     (<c>/bulk_imports/entities</c>, <c>/bulk_imports/:import_id/entities</c>). A migration of a group
///     with nested projects fans out into one entity per namespace and per project.
/// </summary>
public sealed record GitLabBulkImportEntity
{
    public required long Id { get; init; }

    /// <summary>The migration this entity belongs to - the <c>import_id</c> of every entity route.</summary>
    public long? BulkImportId { get; init; }

    /// <summary>Where this entity is in its lifecycle.</summary>
    public GitLabBulkImportStatus? Status { get; init; }

    /// <summary>Whether this entity migrates a group or a project.</summary>
    public GitLabBulkImportEntityType? EntityType { get; init; }

    /// <summary>The full path of the entity on the source instance.</summary>
    public string? SourceFullPath { get; init; }

    /// <summary>The full path the entity is being created at on this instance.</summary>
    public string? DestinationFullPath { get; init; }

    /// <summary>
    ///     The destination slug under its historical name. GitLab deprecated the <c>destination_name</c>
    ///     parameter in favour of <c>destination_slug</c>, but still reports both.
    /// </summary>
    public string? DestinationName { get; init; }

    /// <summary>The last path component the entity is being created under.</summary>
    public string? DestinationSlug { get; init; }

    /// <summary>The namespace the entity is being created in on this instance.</summary>
    public string? DestinationNamespace { get; init; }

    /// <summary>The entity this one is nested under, for a group migrated as part of a larger tree.</summary>
    public long? ParentId { get; init; }

    /// <summary>The group created on this instance, once it exists.</summary>
    public long? NamespaceId { get; init; }

    /// <summary>The project created on this instance, once it exists.</summary>
    public long? ProjectId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     The records this entity could not import. GitLab embeds only the most recent handful here;
    ///     the failures route is the paged, complete list.
    /// </summary>
    public IReadOnlyList<GitLabBulkImportEntityFailure>? Failures { get; init; }

    /// <summary>Whether a group migration was asked to include the projects nested under it.</summary>
    public bool? MigrateProjects { get; init; }

    /// <summary>Whether the migration was asked to carry memberships across.</summary>
    public bool? MigrateMemberships { get; init; }

    /// <summary>Whether this entity recorded any failure.</summary>
    public bool? HasFailures { get; init; }

    /// <summary>
    ///     Per-relation source and imported counts. The spec types this as a bare <c>object</c> whose keys
    ///     are relation names that grow with every GitLab release, so it is kept as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Stats { get; init; }
}