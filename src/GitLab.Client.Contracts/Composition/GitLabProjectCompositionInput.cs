using GitLab.Client.Models;
using GitLab.Client.Models.Responses;

namespace GitLab.Client.Composition;

/// <summary>
///     Route results supplied to <see cref="GitLabProjectCompositionMapper" /> by a composition executor.
/// </summary>
/// <remarks>
///     Each optional member is the exact DTO returned by an existing GitLab route. A null collection means that
///     its route was not obtained; a loaded route with no items must use an empty collection. This lets the mapper
///     detect accidental partial compositions instead of silently treating a missing request as an empty result.
/// </remarks>
public sealed record GitLabProjectCompositionInput
{
    /// <summary>The result of <c>GET /projects/:id</c>.</summary>
    public required GitLabProject Project { get; init; }

    /// <summary>The selected result from <c>GET /projects/:id/pipelines</c>; null is valid when none exists.</summary>
    public GitLabPipeline? LatestPipeline { get; init; }

    /// <summary>The result of <c>GET /projects/:id/pipelines/:pipeline_id/jobs</c>.</summary>
    public IReadOnlyList<GitLabJob>? LatestPipelineJobs { get; init; }

    /// <summary>The result of <c>GET /projects/:id/merge_requests</c>.</summary>
    public IReadOnlyList<GitLabMergeRequest>? MergeRequests { get; init; }

    /// <summary>The result of <c>GET /projects/:id/issues</c>.</summary>
    public IReadOnlyList<GitLabIssue>? Issues { get; init; }

    /// <summary>The result of <c>GET /projects/:id/environments</c>.</summary>
    public IReadOnlyList<GitLabEnvironment>? Environments { get; init; }
}