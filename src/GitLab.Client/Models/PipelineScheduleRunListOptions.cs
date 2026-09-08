using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing the pipelines a schedule has triggered
///     (<c>GET /projects/:id/pipeline_schedules/:pipeline_schedule_id/pipelines</c>).
/// </summary>
[GitLabQuery]
public readonly record struct PipelineScheduleRunListOptions
{
    /// <summary>Pipeline scope - "running", "pending", "finished", "branches" or "tags".</summary>
    public string? Scope { get; init; }

    /// <summary>
    ///     Pipeline status - "created", "waiting_for_resource", "preparing", "waiting_for_callback", "pending",
    ///     "running", "success", "failed", "canceling", "canceled", "skipped" or "manual".
    /// </summary>
    public string? Status { get; init; }

    /// <summary>"asc" or "desc".</summary>
    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}