using GitLab.Client.Models;
using GitLab.Client.Models.Responses;

namespace GitLab.Client.Composition;

/// <summary>
///     A rich, client-side project aggregate assembled from several GitLab route responses.
/// </summary>
/// <remarks>
///     Empty lists are meaningful only when <see cref="IsLoaded" /> reports their corresponding section as loaded.
///     The composition deliberately retains the input <see cref="IReadOnlyList{T}" /> instances: route results are
///     already materialized by the executor, and copying every list during assembly would add a second allocation
///     and traversal to a high-volume SDK path. Executors must therefore not mutate a list after handing it to the
///     mapper.
/// </remarks>
public sealed record GitLabProjectComposition
{
    /// <summary>The plan that describes which route slices were assembled.</summary>
    public required GitLabProjectCompositionLoadPlan LoadPlan { get; init; }

    /// <summary>The root project response.</summary>
    public required GitLabProject Project { get; init; }

    /// <summary>The selected latest pipeline, or null when the loaded pipeline route had no matching result.</summary>
    public GitLabPipeline? LatestPipeline { get; init; }

    /// <summary>The selected pipeline's jobs, or an empty list when that route was not selected or returned no jobs.</summary>
    public IReadOnlyList<GitLabJob> LatestPipelineJobs { get; init; } = [];

    /// <summary>The merge requests, or an empty list when their route was not selected or returned no results.</summary>
    public IReadOnlyList<GitLabMergeRequest> MergeRequests { get; init; } = [];

    /// <summary>The issues, or an empty list when their route was not selected or returned no results.</summary>
    public IReadOnlyList<GitLabIssue> Issues { get; init; } = [];

    /// <summary>The environments, or an empty list when their route was not selected or returned no results.</summary>
    public IReadOnlyList<GitLabEnvironment> Environments { get; init; } = [];

    /// <summary>
    ///     Returns whether the requested route result was loaded. This distinguishes an empty GitLab response from
    ///     a slice intentionally omitted from the plan.
    /// </summary>
    public bool IsLoaded(GitLabProjectCompositionSection section)
    {
        return LoadPlan.Includes(section);
    }
}