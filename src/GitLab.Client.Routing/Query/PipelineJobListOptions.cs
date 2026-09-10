using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for jobs belonging to a single pipeline
///     (<c>GET /projects/:id/pipelines/:pipeline_id/jobs</c>).
/// </summary>
[GitLabQuery]
public readonly record struct PipelineJobListOptions
{
    /// <summary>Includes previous attempts of retried jobs when set to <see langword="true" />.</summary>
    public bool? IncludeRetried { get; init; }

    /// <summary>
    ///     Job statuses to include. GitLab expects repeated <c>scope[]</c> keys instead of a comma-separated
    ///     value.
    /// </summary>
    [QueryParameter("scope", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<string>? Scope { get; init; }

    /// <summary>The number of jobs returned per page.</summary>
    public int? PerPage { get; init; }
}