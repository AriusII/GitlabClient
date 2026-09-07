using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing projects. The spec declares the same parameter set on every project listing,
///     so this one record serves <c>GET /projects</c>, <c>GET /projects/:id/forks</c>,
///     <c>GET /users/:user_id/projects</c>, <c>GET /users/:user_id/starred_projects</c> and
///     <c>GET /users/:user_id/contributed_projects</c> - the last of which recognises only
///     <see cref="OrderBy" />, <see cref="Sort" />, <see cref="Simple" /> and <see cref="PerPage" />.
/// </summary>
/// <remarks>
///     <c>page</c> is deliberately absent: paging is the transport's job, and
///     <c>IGitLabApiConnection.GetPagedAsync</c> follows the <c>Link: rel="next"</c> header rather than
///     counting pages. <c>custom_attributes</c> is absent too - it is a free-form object, which a query
///     string built from typed properties cannot express.
/// </remarks>
[GitLabQuery]
public sealed record ProjectListOptions
{
    public string? Search { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    public IReadOnlyList<string>? Topic { get; init; }

    public bool? Archived { get; init; }

    public DateTimeOffset? LastActivityAfter { get; init; }

    /// <summary>A calendar date, not an instant — the spec declares this one as <c>format: date</c>.</summary>
    public DateOnly? MarkedForDeletionOn { get; init; }

    public int? PerPage { get; init; }

    /// <summary>
    ///     "id", "name", "path", "created_at", "updated_at", "last_activity_at", "similarity",
    ///     "star_count", "storage_size", "repository_size", "wiki_size" or "packages_size". The size
    ///     orderings are administrator-only, and "similarity" only means anything alongside
    ///     <see cref="Search" />.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>"asc" or "desc".</summary>
    public string? Sort { get; init; }

    /// <summary>Match <see cref="Search" /> against ancestor namespace names as well as the project's own.</summary>
    public bool? SearchNamespaces { get; init; }

    /// <summary>Limit to projects owned by the authenticated user.</summary>
    public bool? Owned { get; init; }

    /// <summary>Limit to projects the authenticated user has starred.</summary>
    public bool? Starred { get; init; }

    /// <summary>Limit to projects the authenticated user imported.</summary>
    public bool? Imported { get; init; }

    /// <summary>Limit to projects the authenticated user is a member of.</summary>
    public bool? Membership { get; init; }

    /// <summary>Limit to projects with the issue tracker enabled.</summary>
    public bool? WithIssuesEnabled { get; init; }

    /// <summary>Limit to projects with merge requests enabled.</summary>
    public bool? WithMergeRequestsEnabled { get; init; }

    /// <summary>Limit to projects using this programming language.</summary>
    public string? WithProgrammingLanguage { get; init; }

    /// <summary>Limit to projects where the caller has at least this access level.</summary>
    public int? MinAccessLevel { get; init; }

    /// <summary>Limit to projects with an ID strictly greater than this one.</summary>
    public long? IdAfter { get; init; }

    /// <summary>Limit to projects with an ID strictly smaller than this one.</summary>
    public long? IdBefore { get; init; }

    /// <summary>Limit to projects last active before this instant.</summary>
    public DateTimeOffset? LastActivityBefore { get; init; }

    /// <summary>Limit to projects on this Gitaly storage shard. Administrators only.</summary>
    public string? RepositoryStorage { get; init; }

    /// <summary>Limit to projects carrying the topic with this ID.</summary>
    public long? TopicId { get; init; }

    /// <summary>Limit to projects updated before this instant.</summary>
    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <summary>Limit to projects updated after this instant.</summary>
    public DateTimeOffset? UpdatedAfter { get; init; }

    /// <summary>Include projects already marked for deletion.</summary>
    public bool? IncludePendingDelete { get; init; }

    /// <summary>Limit to projects that are neither archived nor marked for deletion.</summary>
    public bool? Active { get; init; }

    /// <summary>Limit to projects whose wiki failed its last checksum. Administrators only.</summary>
    public bool? WikiChecksumFailed { get; init; }

    /// <summary>Limit to projects whose repository failed its last checksum. Administrators only.</summary>
    public bool? RepositoryChecksumFailed { get; init; }

    /// <summary>Include projects hidden from the UI. Administrators only.</summary>
    public bool? IncludeHidden { get; init; }

    /// <summary>Return only each project's ID, URL, name and path.</summary>
    public bool? Simple { get; init; }

    /// <summary>Include each project's storage statistics. Administrators only.</summary>
    public bool? Statistics { get; init; }

    /// <summary>Include each project's custom attributes. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }
}