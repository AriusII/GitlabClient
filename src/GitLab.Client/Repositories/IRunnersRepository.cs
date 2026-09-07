using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Runners resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IRunnersService), typeof(IRunnersClient))]
internal interface IRunnersRepository
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