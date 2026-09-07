using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "CI resource groups" API area (<c>/projects/:id/resource_groups</c>) - the
///     mutexes that keep two jobs declaring the same <c>resource_group:</c> from running at once, which
///     is how a project stops concurrent deployments to one environment.
///     <para>
///         There is no create endpoint on purpose: a resource group springs into existence the first time
///         a pipeline references its key, so this surface is read plus a single mutable property, the
///         process mode.
///     </para>
/// </summary>
public interface IResourceGroupsClient
{
    /// <summary>Streams every resource group a pipeline has ever created on the project.</summary>
    IAsyncEnumerable<GitLabResourceGroup> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one resource group by key. Keys are free text from <c>.gitlab-ci.yml</c> and legally contain
    ///     <c>/</c>; the route builder percent-encodes the key, so pass it raw.
    /// </summary>
    Task<GitLabResourceGroup> GetAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Changes the order in which the resource group releases waiting jobs. GitLab answers <c>400</c>
    ///     rather than <c>422</c> for an unrecognised process mode.
    /// </summary>
    Task<GitLabResourceGroup> UpdateAsync(ProjectId projectId, string key, UpdateResourceGroupRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the job currently holding the resource group. Nothing holds a resource group most of the
    ///     time, and GitLab has no "empty" representation for this endpoint, so expect a
    ///     <see cref="Exceptions.GitLabApiException" /> rather than a null when the group is idle.
    /// </summary>
    Task<GitLabJob> GetCurrentJobAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the jobs queued behind the current holder, in the order the process mode will release them.</summary>
    IAsyncEnumerable<GitLabJob> ListUpcomingJobsAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default);
}