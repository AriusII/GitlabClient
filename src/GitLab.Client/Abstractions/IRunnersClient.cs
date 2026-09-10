using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

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

    /// <summary>
    ///     Updates a runner's configuration - description, pause state, tags, and similar settings. Omitted
    ///     members keep their current server-side value.
    /// </summary>
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

    /// <summary>
    ///     Registers a runner process through the legacy runner protocol (<c>POST /runners</c>) and returns
    ///     its newly issued authentication token. For creating a runner owned by the current GitLab user,
    ///     prefer <see cref="ICurrentUserClient.CreateRunnerAsync" />.
    /// </summary>
    /// <param name="request">The registration token and the runner process's initial configuration.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The registered runner's id and write-once authentication token.</returns>
    Task<GitLabRunnerRegistration> RegisterAsync(RegisterRunnerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Revokes a runner registration using the runner's own authentication token
    ///     (<c>DELETE /runners?token=…</c>). This is the runner-protocol counterpart to
    ///     <see cref="DeleteAsync" />, which removes a runner by its administrative id.
    /// </summary>
    /// <param name="token">The runner authentication token to revoke.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task UnregisterAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one manager registration from a runner using its authentication token and system id
    ///     (<c>DELETE /runners/managers</c>). It does not delete the runner itself.
    /// </summary>
    /// <param name="token">The runner authentication token.</param>
    /// <param name="systemId">The runner manager's system identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task RemoveManagerAsync(string token, string systemId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Verifies a runner authentication token. GitLab returns the runner id and the same registration
    ///     details when the credential remains valid; a 403 or 422 means that the credential cannot be used.
    /// </summary>
    /// <param name="request">The runner token and, optionally, its manager system id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabRunnerRegistration> VerifyAsync(VerifyRunnerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rotates the authentication token that is supplied in the request body. This is intended for a
    ///     runner rotating its own credential; use <see cref="ResetAuthenticationTokenAsync(long, CancellationToken)" />
    ///     when an administrator is rotating another runner's token by id.
    /// </summary>
    /// <param name="request">The runner's current authentication token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The new write-once authentication token.</returns>
    Task<GitLabRunnerToken> ResetAuthenticationTokenAsync(ResetRunnerAuthenticationTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discovers the secure Job Router endpoint available to the authenticated runner
    ///     (<c>GET /runners/router/discovery</c>). The returned WebSocket URI is optional because an instance
    ///     can have the Job Router disabled.
    /// </summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabRunnerRouterDiscovery> DiscoverJobRouterAsync(CancellationToken cancellationToken = default);
}