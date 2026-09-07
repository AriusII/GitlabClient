using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's deployments (<c>GET /projects/:id/deployments</c>).</summary>
[GitLabQuery]
public sealed record DeploymentListOptions
{
    /// <summary>One of "id", "iid", "created_at", "updated_at" or "finished_at". GitLab defaults to "id".</summary>
    public string? OrderBy { get; init; }

    /// <summary>Either "asc" or "desc".</summary>
    public string? Sort { get; init; }

    /// <summary>Restricts the result to deployments to the named environment.</summary>
    public string? Environment { get; init; }

    /// <summary>
    ///     One of "created", "running", "success", "failed", "canceled", "skipped" or "blocked".
    /// </summary>
    public string? Status { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? FinishedAfter { get; init; }

    public DateTimeOffset? FinishedBefore { get; init; }

    public int? PerPage { get; init; }
}