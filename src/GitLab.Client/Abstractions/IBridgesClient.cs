using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Compatibility wrapper for GitLab's legacy pipeline bridge endpoint
///     (<c>/projects/:id/pipelines/:pipeline_id/bridges</c>).
/// </summary>
/// <remarks>
///     GitLab deprecated this endpoint in 19.2 in favour of <see cref="IPipelinesClient.ListTriggerJobsAsync" />.
///     The response and query contracts remain identical, so this client intentionally shares
///     <see cref="TriggerJobListOptions" /> and <see cref="GitLabBridge" /> instead of introducing duplicate
///     DTOs solely for the legacy route. Prefer <see cref="IPipelinesClient.ListTriggerJobsAsync" /> for new
///     code; this client exists for instances and callers that still expose the documented compatibility path.
/// </remarks>
public interface IBridgesClient
{
    /// <summary>
    ///     Lists a pipeline's bridge jobs through GitLab's legacy <c>bridges</c> route, streaming every page.
    ///     Prefer <see cref="IPipelinesClient.ListTriggerJobsAsync" />, which uses GitLab's supported
    ///     <c>trigger_jobs</c> replacement route.
    /// </summary>
    IAsyncEnumerable<GitLabBridge> ListAsync(ProjectId projectId, long pipelineId,
        TriggerJobListOptions? options = null, CancellationToken cancellationToken = default);
}