using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Projects job token scope" API area (<c>/projects/:id/job_token_scope</c>) - the
///     CI/CD job token settings that control which other projects and groups a job token minted in
///     <em>this</em> project is allowed to authenticate against.
///     <para>
///         Every operation here requires the Maintainer or Owner role on the project; anything less
///         answers <c>403</c>.
///     </para>
/// </summary>
public interface IJobTokenScopeClient
{
    /// <summary>Reads whether inbound/outbound job token access restriction is currently enabled for the project.</summary>
    Task<GitLabProjectJobTokenScope> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Turns the project's job token access restriction on or off. With it enabled, only the projects
    ///     and groups on the allowlists below (plus the project itself) can be authenticated against using a
    ///     CI/CD job token minted in another project.
    /// </summary>
    Task UpdateAsync(ProjectId projectId, UpdateProjectJobTokenScopeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every project on this project's CI/CD job token allowlist.</summary>
    IAsyncEnumerable<GitLabProject> ListAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Adds a project to this project's CI/CD job token allowlist.</summary>
    Task<GitLabProject> AddToAllowlistAsync(ProjectId projectId, AddProjectToJobTokenAllowlistRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a project from this project's CI/CD job token allowlist.</summary>
    Task RemoveFromAllowlistAsync(ProjectId projectId, long targetProjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every group on this project's CI/CD job token allowlist.
    ///     <para>
    ///         The pinned spec documents this endpoint's response as a project payload
    ///         (<c>APIEntitiesBasicProjectDetails</c>), which does not match its own sibling operations -
    ///         the add and remove endpoints on this same allowlist both work in groups
    ///         (<c>APIEntitiesBasicGroupDetails</c> / a group id), and GitLab genuinely returns group
    ///         objects here. This method follows the real, group-shaped payload rather than the
    ///         apparently mistyped spec entry.
    ///     </para>
    /// </summary>
    IAsyncEnumerable<GitLabJobTokenScopeGroup> ListGroupsAllowlistAsync(ProjectId projectId,
        JobTokenScopeAllowlistListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Adds a group to this project's CI/CD job token allowlist.</summary>
    Task<GitLabJobTokenScopeGroup> AddGroupToAllowlistAsync(ProjectId projectId,
        AddGroupToJobTokenAllowlistRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a group from this project's CI/CD job token allowlist.</summary>
    Task RemoveGroupFromAllowlistAsync(ProjectId projectId, long targetGroupId,
        CancellationToken cancellationToken = default);
}