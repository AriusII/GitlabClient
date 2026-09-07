using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing the jobs a runner has processed (<c>GET /runners/:id/jobs</c>).</summary>
[GitLabQuery]
public sealed record RunnerJobListOptions
{
    /// <summary>
    ///     Job status - "created", "waiting_for_resource", "preparing", "waiting_for_callback", "pending",
    ///     "running", "success", "failed", "canceling", "canceled", "skipped" or "manual".
    /// </summary>
    public string? Status { get; init; }

    /// <summary>GitLab accepts only "id" here; pair it with <see cref="Sort" />.</summary>
    public string? OrderBy { get; init; }

    /// <summary>"asc" or "desc". GitLab wants <see cref="OrderBy" /> supplied alongside it, even for "id".</summary>
    public string? Sort { get; init; }

    /// <summary>Restrict to the jobs handled by one runner manager, identified by its system id.</summary>
    public string? SystemId { get; init; }

    public int? PerPage { get; init; }
}