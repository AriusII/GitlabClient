using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The state of a project import (<c>GET /projects/:id/import</c>), and the shape every archive-based
///     import endpoint answers with - <c>POST /projects/import</c>, <c>POST /projects/remote-import</c>,
///     <c>POST /projects/remote-import-s3</c> and <c>POST /projects/:id/import/git</c>.
/// </summary>
public sealed record GitLabProjectImportStatus
{
    public required long Id { get; init; }

    public string? Description { get; init; }

    public string? Name { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     Where the import has got to - <c>none</c>, <c>scheduled</c>, <c>started</c>, <c>finished</c>,
    ///     <c>failed</c>, <c>canceled</c>.
    ///     <para>
    ///         Deliberately a <see cref="string" /> and not an enum: the spec types this field as a bare
    ///         string with no enumeration, and GitLab has grown the vocabulary before. A value it adds later
    ///         must not turn a healthy response into a <see cref="JsonException" />.
    ///     </para>
    /// </summary>
    public string? ImportStatus { get; init; }

    /// <summary>Which importer ran - <c>gitlab_project</c> for an archive import, <c>github</c>, <c>git</c>, and so on.</summary>
    public string? ImportType { get; init; }

    /// <summary>The correlation id of the import job, for matching against GitLab's own logs when it fails.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Relations the importer could not restore. Empty or null on a clean import.</summary>
    public IReadOnlyList<GitLabProjectImportFailedRelation>? FailedRelations { get; init; }

    /// <summary>The failure message when <see cref="ImportStatus" /> is <c>failed</c>.</summary>
    public string? ImportError { get; init; }

    /// <summary>
    ///     Per-relation counts of what was fetched and what was imported. The spec types it as an untyped
    ///     object whose keys are the relation names of whichever importer ran, so it is captured as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Stats { get; init; }
}