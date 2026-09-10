using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Projects" API area: <c>/projects</c> and its per-project sub-routes
///     (archive, fork, star, share, transfer, housekeeping, security settings, audit events), plus the
///     three project listings that hang off <c>/users/:user_id</c>.
/// </summary>
public interface IProjectsClient
{
    /// <summary>Retrieves one project by its numeric ID or URL-encoded namespaced path (<c>GET /projects/:id</c>).</summary>
    Task<GitLabProject> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams every project visible to the caller (<c>GET /projects</c>), following the pagination
    ///     links. Unfiltered, this is every public project on the instance - use
    ///     <see cref="ProjectListOptions.Owned" /> or <see cref="ProjectListOptions.Membership" /> to
    ///     narrow it to the caller's own projects.
    /// </summary>
    IAsyncEnumerable<GitLabProject> ListAsync(ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project in the authenticated user's own namespace, or the one named in the request
    ///     (<c>POST /projects</c>).
    /// </summary>
    Task<GitLabProject> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a project owned by another user (<c>POST /projects/user/:user_id</c>). Administrators
    ///     only.
    /// </summary>
    Task<GitLabProject> CreateForUserAsync(long userId, CreateProjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies a partial update to a project (<c>PUT /projects/:id</c>). Only the members set on the
    ///     request are sent; anything left null keeps its current value.
    /// </summary>
    Task<GitLabProject> UpdateAsync(ProjectId projectId, UpdateProjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets or replaces the project's avatar (<c>PUT /projects/:id</c> with a multipart body). The
    ///     stream is read but never disposed, so the caller keeps ownership of it.
    /// </summary>
    Task<GitLabProject> SetAvatarAsync(ProjectId projectId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a project (<c>DELETE /projects/:id</c>). On instances with deletion delay enabled this
    ///     marks the project for deletion rather than removing it immediately - see
    ///     <see cref="RestoreAsync" /> to undo it before the delay elapses.
    /// </summary>
    Task DeleteAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Restores a project that has been marked for deletion (<c>POST /projects/:id/restore</c>),
    ///     undoing the pending <see cref="DeleteAsync" />.
    /// </summary>
    Task<GitLabProject> RestoreAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Archives a project, making it read-only (<c>POST /projects/:id/archive</c>).</summary>
    Task<GitLabProject> ArchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Reverses <see cref="ArchiveAsync" /> (<c>POST /projects/:id/unarchive</c>).</summary>
    Task<GitLabProject> UnarchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Stars a project as the authenticated user (<c>POST /projects/:id/star</c>).</summary>
    Task<GitLabProject> StarAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Reverses <see cref="StarAsync" /> (<c>POST /projects/:id/unstar</c>).</summary>
    Task<GitLabProject> UnstarAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users who starred the project, and when (<c>GET /projects/:id/starrers</c>).
    /// </summary>
    IAsyncEnumerable<GitLabProjectStarrer> ListStarrersAsync(ProjectId projectId,
        ProjectStarrerListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Forks the project (<c>POST /projects/:id/fork</c>). A null <paramref name="request" /> sends no
    ///     body and forks into the authenticated user's own namespace under the source project's name and
    ///     path.
    /// </summary>
    Task<GitLabProject> ForkAsync(ProjectId projectId, ForkProjectRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the project's forks (<c>GET /projects/:id/forks</c>).</summary>
    IAsyncEnumerable<GitLabProject> ListForksAsync(ProjectId projectId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks an already-existing project as a fork of another
    ///     (<c>POST /projects/:id/fork/:forked_from_id</c>), without copying any content - the inverse of
    ///     <see cref="DeleteForkRelationshipAsync" />.
    /// </summary>
    Task<GitLabProject> CreateForkRelationshipAsync(ProjectId projectId, ProjectId forkedFromId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the project's fork relationship (<c>DELETE /projects/:id/fork</c>), leaving the project
    ///     itself untouched.
    /// </summary>
    Task DeleteForkRelationshipAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Moves a project into a different namespace (<c>PUT /projects/:id/transfer</c>).</summary>
    Task<GitLabProject> TransferAsync(ProjectId projectId, TransferProjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the namespaces the authenticated user could transfer the project into
    ///     (<c>GET /projects/:id/transfer_locations</c>).
    /// </summary>
    IAsyncEnumerable<GitLabPublicGroupDetails> ListTransferLocationsAsync(ProjectId projectId,
        ProjectTransferLocationListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the project's ancestor groups (<c>GET /projects/:id/groups</c>) - its own namespace and
    ///     every parent group above it, optionally including the groups it is shared with.
    /// </summary>
    IAsyncEnumerable<GitLabPublicGroupDetails> ListAncestorGroupsAsync(ProjectId projectId,
        ProjectAncestorGroupListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the groups the project has been shared with (<c>GET /projects/:id/invited_groups</c>),
    ///     as opposed to <see cref="ListAncestorGroupsAsync" />'s ownership hierarchy.
    /// </summary>
    IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(ProjectId projectId,
        ProjectInvitedGroupListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the groups the project could still be shared with
    ///     (<c>GET /projects/:id/share_locations</c>) - the candidates for <see cref="ShareAsync" />.
    /// </summary>
    IAsyncEnumerable<GitLabGroup> ListShareLocationsAsync(ProjectId projectId,
        ProjectShareLocationListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Shares the project with a group (<c>POST /projects/:id/share</c>), granting the group's members
    ///     access at the given level.
    /// </summary>
    Task<GitLabProjectGroupLink> ShareAsync(ProjectId projectId, ShareProjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a group's share link, revoking the access <see cref="ShareAsync" /> granted
    ///     (<c>DELETE /projects/:id/share/:group_id</c>).
    /// </summary>
    Task UnshareAsync(ProjectId projectId, long groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users associated with the project - members and, for public projects, everyone who
    ///     can be <c>@mentioned</c> - for autocomplete-style lookups (<c>GET /projects/:id/users</c>).
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListUsersAsync(ProjectId projectId, ProjectUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Imports the members of another project the caller can administer into this one
    ///     (<c>POST /projects/:id/import_project_members/:source_project_id</c>).
    /// </summary>
    Task ImportMembersAsync(ProjectId projectId, long sourceProjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the repository's programming languages as a map from language name to the percentage of
    ///     the codebase it makes up (<c>GET /projects/:id/languages</c>).
    /// </summary>
    Task<IReadOnlyDictionary<string, double>> GetLanguagesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the project's repository fetch statistics over the last 30 days
    ///     (<c>GET /projects/:id/statistics</c>).
    /// </summary>
    Task<GitLabProjectDailyStatistics> GetStatisticsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns where the project's repository physically lives on Gitaly
    ///     (<c>GET /projects/:id/storage</c>). Administrators only.
    /// </summary>
    Task<GitLabProjectStorage> GetStorageAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Triggers a Git housekeeping run on the project's repository
    ///     (<c>POST /projects/:id/housekeeping</c>). A null <paramref name="request" /> sends no body, so
    ///     GitLab picks whichever task the project is due for.
    /// </summary>
    Task StartHousekeepingAsync(ProjectId projectId, ProjectHousekeepingRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules a recalculation of the project's repository and wiki storage size
    ///     (<c>POST /projects/:id/repository_size</c>).
    /// </summary>
    Task RecalculateRepositorySizeAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the project's security settings (<c>GET /projects/:id/security_settings</c>).</summary>
    Task<GitLabProjectSecuritySettings> GetSecuritySettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the project's security settings (<c>PUT /projects/:id/security_settings</c>). Only the
    ///     members set on the request are sent; anything left null keeps its current value.
    /// </summary>
    Task<GitLabProjectSecuritySettings> UpdateSecuritySettingsAsync(ProjectId projectId,
        UpdateProjectSecuritySettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Opens a merge request that adds a starter <c>.gitlab-ci.yml</c> to the project
    ///     (<c>POST /projects/:id/create_ci_config</c>).
    /// </summary>
    Task<GitLabMergeRequest> CreateCiConfigMergeRequestAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the project's audit log (<c>GET /projects/:id/audit_events</c>).</summary>
    IAsyncEnumerable<GitLabProjectAuditEvent> ListAuditEventsAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one entry of the project's audit log by its ID
    ///     (<c>GET /projects/:id/audit_events/:audit_event_id</c>).
    /// </summary>
    Task<GitLabProjectAuditEvent> GetAuditEventAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the projects owned by a user (<c>GET /users/:user_id/projects</c>).
    ///     <paramref name="userId" /> accepts a numeric ID or a username.
    /// </summary>
    IAsyncEnumerable<GitLabProject> ListForUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the projects a user has starred (<c>GET /users/:user_id/starred_projects</c>).
    ///     <paramref name="userId" /> accepts a numeric ID or a username.
    /// </summary>
    IAsyncEnumerable<GitLabProject> ListStarredByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the projects a user has contributed to (<c>GET /users/:user_id/contributed_projects</c>).
    ///     <paramref name="userId" /> accepts a numeric ID or a username.
    /// </summary>
    IAsyncEnumerable<GitLabProject> ListContributedByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);
}