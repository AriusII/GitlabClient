using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a project's pipeline schedules (<c>GET /projects/:id/pipeline_schedules</c>).</summary>
[GitLabQuery]
public readonly record struct PipelineScheduleListOptions
{
    /// <summary>Which schedules to return - "active" or "inactive". All schedules are returned when omitted.</summary>
    public string? Scope { get; init; }

    /// <summary>The one-based result page to retrieve.</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}