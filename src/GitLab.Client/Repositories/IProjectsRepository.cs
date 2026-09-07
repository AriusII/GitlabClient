using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Projects resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IProjectsService), typeof(IProjectsClient))]
internal interface IProjectsRepository
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