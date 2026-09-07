using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Projects" API area: <c>/projects</c> and its per-project sub-routes
///     (archive, fork, star, share, transfer, housekeeping, security settings, audit events), plus the
///     three project listings that hang off <c>/users/:user_id</c>.
/// </summary>
public interface IProjectsClient
{
    Task<GitLabProject> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListAsync(ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabProject> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    Task<GitLabProject> CreateForUserAsync(long userId, CreateProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProject> UpdateAsync(ProjectId projectId, UpdateProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabProject> SetAvatarAsync(ProjectId projectId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> RestoreAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> ArchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> UnarchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> StarAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> UnstarAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProjectStarrer> ListStarrersAsync(ProjectId projectId,
        ProjectStarrerListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProject> ForkAsync(ProjectId projectId, ForkProjectRequest? request = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListForksAsync(ProjectId projectId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabProject> CreateForkRelationshipAsync(ProjectId projectId, ProjectId forkedFromId,
        CancellationToken cancellationToken = default);

    Task DeleteForkRelationshipAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProject> TransferAsync(ProjectId projectId, TransferProjectRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPublicGroupDetails> ListTransferLocationsAsync(ProjectId projectId,
        ProjectTransferLocationListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabPublicGroupDetails> ListAncestorGroupsAsync(ProjectId projectId,
        ProjectAncestorGroupListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(ProjectId projectId,
        ProjectInvitedGroupListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListShareLocationsAsync(ProjectId projectId,
        ProjectShareLocationListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProjectGroupLink> ShareAsync(ProjectId projectId, ShareProjectRequest request,
        CancellationToken cancellationToken = default);

    Task UnshareAsync(ProjectId projectId, long groupId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListUsersAsync(ProjectId projectId, ProjectUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task ImportMembersAsync(ProjectId projectId, long sourceProjectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, double>> GetLanguagesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectDailyStatistics> GetStatisticsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectStorage> GetStorageAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task StartHousekeepingAsync(ProjectId projectId, ProjectHousekeepingRequest? request = null,
        CancellationToken cancellationToken = default);

    Task RecalculateRepositorySizeAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabProjectSecuritySettings> GetSecuritySettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProjectSecuritySettings> UpdateSecuritySettingsAsync(ProjectId projectId,
        UpdateProjectSecuritySettingsRequest request, CancellationToken cancellationToken = default);

    Task<GitLabMergeRequest> CreateCiConfigMergeRequestAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProjectAuditEvent> ListAuditEventsAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabProjectAuditEvent> GetAuditEventAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListForUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListStarredByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListContributedByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default);
}