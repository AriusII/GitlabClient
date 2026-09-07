using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Runners" API area (<c>/runners</c>, <c>/projects/:id/runners</c>,
///     <c>/groups/:id/runners</c>): inventorying runners, reading and updating one, moving project runners
///     between projects, and listing what a runner has executed.
/// </summary>
public interface IRunnersClient
{
    /// <summary>Lists the runners available to the calling user, streaming every page.</summary>
    IAsyncEnumerable<GitLabRunner> ListAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every runner in the instance (<c>GET /runners/all</c>). Requires administrator or auditor
    ///     access; anything less is answered with a <see cref="Exceptions.GitLabForbiddenException" />.
    /// </summary>
    IAsyncEnumerable<GitLabRunner> ListAllAsync(RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the runners available in a project, including inherited group and instance runners.</summary>
    IAsyncEnumerable<GitLabRunner> ListProjectRunnersAsync(ProjectId projectId, RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the runners available in a group, including those inherited from ancestor groups.</summary>
    IAsyncEnumerable<GitLabRunner> ListGroupRunnersAsync(GroupId groupId, RunnerListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a runner's details. Instance-runner details are readable by any authenticated user; for a
    ///     group or project runner you need the Maintainer or Owner role on the associated group or project.
    /// </summary>
    /// <param name="runnerId">The runner's numeric id.</param>
    /// <param name="includeProjects">
    ///     Pass <see langword="false" /> to omit the runner's project list - GitLab's documented performance
    ///     escape hatch for a runner assigned to many projects.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabRunner> GetAsync(long runnerId, bool? includeProjects = null,
        CancellationToken cancellationToken = default);

    Task<GitLabRunner> UpdateAsync(long runnerId, UpdateRunnerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a runner outright. GitLab echoes the deleted runner back; the response is discarded.</summary>
    Task DeleteAsync(long runnerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Assigns an <em>existing</em> project runner to another project. This does not create a runner -
    ///     runners are created through <c>POST /user/runners</c>, on the Users API area.
    /// </summary>
    Task<GitLabRunner> AssignToProjectAsync(ProjectId projectId, AssignRunnerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unassigns a project runner from a project. A runner cannot be unassigned from its owner project -
    ///     GitLab answers that with a <see cref="Exceptions.GitLabValidationException" />; use
    ///     <see cref="DeleteAsync" /> instead.
    /// </summary>
    Task UnassignFromProjectAsync(ProjectId projectId, long runnerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the jobs a runner is processing or has processed, streaming every page. Limited to projects
    ///     where the caller has at least the Reporter role.
    /// </summary>
    IAsyncEnumerable<GitLabJob> ListJobsAsync(long runnerId, RunnerJobListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the runner managers behind a runner, streaming every page. One runner registration can be
    ///     started on many machines; each machine reports itself as a manager with its own
    ///     <see cref="GitLabRunnerManager.SystemId" />, version and contact time.
    /// </summary>
    IAsyncEnumerable<GitLabRunnerManager> ListManagersAsync(long runnerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the projects a runner is assigned to, streaming every page. Meaningful only for a project
    ///     runner - an instance or group runner reports no assignments here.
    /// </summary>
    IAsyncEnumerable<GitLabProject> ListProjectsAsync(long runnerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resets the instance-wide runner registration token. Administrators only.
    ///     <para>
    ///         The previous token stops working immediately, and the new one is returned exactly once - see
    ///         <see cref="GitLabRunnerToken.Token" />.
    ///     </para>
    /// </summary>
    Task<GitLabRunnerToken> ResetRegistrationTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resets a project's runner registration token, invalidating the previous one. Requires the Owner
    ///     role on the project. The new token is returned exactly once.
    /// </summary>
    Task<GitLabRunnerToken> ResetProjectRegistrationTokenAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resets a group's runner registration token, invalidating the previous one. Requires the Owner role
    ///     on the group. The new token is returned exactly once.
    /// </summary>
    Task<GitLabRunnerToken> ResetGroupRegistrationTokenAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resets one runner's authentication token, which disconnects every manager currently running under
    ///     it until they are reconfigured. The new token is returned exactly once.
    /// </summary>
    Task<GitLabRunnerToken> ResetAuthenticationTokenAsync(long runnerId, CancellationToken cancellationToken = default);
}