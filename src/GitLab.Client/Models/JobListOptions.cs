using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's jobs (<c>GET /projects/:id/jobs</c>).</summary>
[GitLabQuery]
public sealed record JobListOptions
{
    /// <summary>
    ///     Job statuses to include - "created", "pending", "running", "failed", "success", "canceled",
    ///     "skipped", "manual", "waiting_for_resource", "scheduled". The spec declares this as an array, and
    ///     GitLab expects the repeated form (<c>scope[]=pending&amp;scope[]=running</c>) here rather than the
    ///     comma-joined default.
    /// </summary>
    [QueryParameter("scope", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<string>? Scope { get; init; }

    /// <summary>Return only jobs that ran against this branch or tag name.</summary>
    public string? Ref { get; init; }

    public int? PerPage { get; init; }
}