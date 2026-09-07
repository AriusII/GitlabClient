using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Runners, sitting between the public <c>IRunnersClient</c>
///     controller and <c>IRunnersRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IRunnersService
{
    IAsyncEnumerable<GitLabRunner> ListAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunner> ListAllAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunner> ListProjectRunnersAsync(ProjectId projectId, RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunner> ListGroupRunnersAsync(GroupId groupId, RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabRunner> GetAsync(long runnerId, bool? includeProjects = null,
        CancellationToken cancellationToken = default);

    Task<GitLabRunner> UpdateAsync(long runnerId, UpdateRunnerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long runnerId, CancellationToken cancellationToken = default);

    Task<GitLabRunner> AssignToProjectAsync(ProjectId projectId, AssignRunnerRequest request,
        CancellationToken cancellationToken = default);

    Task UnassignFromProjectAsync(ProjectId projectId, long runnerId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabJob> ListJobsAsync(long runnerId, RunnerJobListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRunnerManager> ListManagersAsync(long runnerId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListProjectsAsync(long runnerId, CancellationToken cancellationToken = default);

    Task<GitLabRunnerToken> ResetRegistrationTokenAsync(CancellationToken cancellationToken = default);

    Task<GitLabRunnerToken> ResetProjectRegistrationTokenAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabRunnerToken> ResetGroupRegistrationTokenAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabRunnerToken> ResetAuthenticationTokenAsync(long runnerId, CancellationToken cancellationToken = default);
}