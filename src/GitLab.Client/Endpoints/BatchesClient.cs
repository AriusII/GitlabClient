using GitLab.Client.Abstractions;
using GitLab.Client.Batching;

namespace GitLab.Client.Endpoints;

/// <summary>Stateless root-client factory for locally coordinated GitLab batch plans.</summary>
/// <remarks>
///     This endpoint intentionally has no transport dependency. A plan receives the caller's existing REST and
///     GraphQL delegates, so every started operation continues through the normal named HttpClient, authentication,
///     retry and rate-limit-observation pipeline.
/// </remarks>
internal sealed class BatchesClient : IBatchesClient
{
    public GitLabBatchPlan Create(GitLabBatchOptions? options = null)
    {
        return new GitLabBatchPlan(options);
    }
}