using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's pipeline schedules (<c>GET /projects/:id/pipeline_schedules</c>).</summary>
[GitLabQuery]
public sealed record PipelineScheduleListOptions
{
    /// <summary>Which schedules to return - "active" or "inactive". All schedules are returned when omitted.</summary>
    public string? Scope { get; init; }

    public int? PerPage { get; init; }
}