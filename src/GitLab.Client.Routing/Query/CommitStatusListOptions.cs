using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a commit's statuses
///     (<c>GET /projects/:id/repository/commits/:sha/statuses</c>).
/// </summary>
[GitLabQuery]
public readonly record struct CommitStatusListOptions
{
    /// <summary>Restrict to statuses reported against this branch or tag.</summary>
    public string? Ref { get; init; }

    public string? Stage { get; init; }

    /// <summary>Restrict to statuses carrying this label.</summary>
    public string? Name { get; init; }

    public long? PipelineId { get; init; }

    /// <summary>
    ///     Return every status rather than only the latest one per name. GitLab defaults this to false, which
    ///     silently hides retried statuses.
    /// </summary>
    public bool? All { get; init; }

    /// <summary>Either "id" or "pipeline_id".</summary>
    public string? OrderBy { get; init; }

    /// <summary>Either "asc" or "desc".</summary>
    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}