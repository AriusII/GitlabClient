using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a pipeline's trigger (bridge) jobs
///     (<c>GET /projects/:id/pipelines/:pipeline_id/trigger_jobs</c>).
/// </summary>
[GitLabQuery]
public readonly record struct TriggerJobListOptions
{
    /// <summary>
    ///     Job statuses to include - "created", "pending", "running", "failed", "success", "canceled",
    ///     "skipped", "manual". GitLab expects the repeated form (<c>scope[]=failed&amp;scope[]=success</c>)
    ///     here, exactly as it does for the jobs endpoint.
    /// </summary>
    [QueryParameter("scope", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<string>? Scope { get; init; }

    /// <summary>The one-based result page to retrieve.</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}