using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Repositories;

namespace GitLab.Client.Tests.TestSupport;

/// <summary>
///     Hand-written stand-in for <c>IProjectsRepository</c>, used by the generated-layer tests. Only the
///     members those tests drive are configurable; the rest of the interface is present because C#
///     requires it and throws if anything reaches for it.
/// </summary>
internal sealed class FakeProjectsRepository : IProjectsRepository
{
    public Func<ProjectId, CancellationToken, Task<GitLabProject>>? OnGetAsync { get; set; }

    public Func<ProjectListOptions?, CancellationToken, IAsyncEnumerable<GitLabProject>>? OnListAsync { get; set; }

    public Task<GitLabProject> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return (OnGetAsync ?? throw new InvalidOperationException($"{nameof(OnGetAsync)} was not configured."))(
            projectId, cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListAsync(ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return (OnListAsync ?? throw new InvalidOperationException($"{nameof(OnListAsync)} was not configured."))(
            options, cancellationToken);
    }

    public Task<GitLabProject> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(CreateAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> CreateForUserAsync(long userId, CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(CreateForUserAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> UpdateAsync(ProjectId projectId, UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(UpdateAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> SetAvatarAsync(ProjectId projectId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(SetAvatarAsync)} is not configured on this fake.");
    }

    public Task DeleteAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(DeleteAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> RestoreAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(RestoreAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> ArchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ArchiveAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> UnarchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(UnarchiveAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> StarAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(StarAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> UnstarAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(UnstarAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProjectStarrer> ListStarrersAsync(ProjectId projectId,
        ProjectStarrerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListStarrersAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> ForkAsync(ProjectId projectId, ForkProjectRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ForkAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProject> ListForksAsync(ProjectId projectId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListForksAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> CreateForkRelationshipAsync(ProjectId projectId, ProjectId forkedFromId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(CreateForkRelationshipAsync)} is not configured on this fake.");
    }

    public Task DeleteForkRelationshipAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(DeleteForkRelationshipAsync)} is not configured on this fake.");
    }

    public Task<GitLabProject> TransferAsync(ProjectId projectId, TransferProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(TransferAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabPublicGroupDetails> ListTransferLocationsAsync(ProjectId projectId,
        ProjectTransferLocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListTransferLocationsAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabPublicGroupDetails> ListAncestorGroupsAsync(ProjectId projectId,
        ProjectAncestorGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListAncestorGroupsAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(ProjectId projectId,
        ProjectInvitedGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListInvitedGroupsAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabGroup> ListShareLocationsAsync(ProjectId projectId,
        ProjectShareLocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListShareLocationsAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectGroupLink> ShareAsync(ProjectId projectId, ShareProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ShareAsync)} is not configured on this fake.");
    }

    public Task UnshareAsync(ProjectId projectId, long groupId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(UnshareAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabUser> ListUsersAsync(ProjectId projectId, ProjectUserListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListUsersAsync)} is not configured on this fake.");
    }

    public Task ImportMembersAsync(ProjectId projectId, long sourceProjectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ImportMembersAsync)} is not configured on this fake.");
    }

    public Task<IReadOnlyDictionary<string, double>> GetLanguagesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(GetLanguagesAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectDailyStatistics> GetStatisticsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(GetStatisticsAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectStorage> GetStorageAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(GetStorageAsync)} is not configured on this fake.");
    }

    public Task StartHousekeepingAsync(ProjectId projectId, ProjectHousekeepingRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(StartHousekeepingAsync)} is not configured on this fake.");
    }

    public Task RecalculateRepositorySizeAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(RecalculateRepositorySizeAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectSecuritySettings> GetSecuritySettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(GetSecuritySettingsAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectSecuritySettings> UpdateSecuritySettingsAsync(ProjectId projectId,
        UpdateProjectSecuritySettingsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(UpdateSecuritySettingsAsync)} is not configured on this fake.");
    }

    public Task<GitLabMergeRequest> CreateCiConfigMergeRequestAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(CreateCiConfigMergeRequestAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProjectAuditEvent> ListAuditEventsAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListAuditEventsAsync)} is not configured on this fake.");
    }

    public Task<GitLabProjectAuditEvent> GetAuditEventAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(GetAuditEventAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProject> ListForUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListForUserAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProject> ListStarredByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListStarredByUserAsync)} is not configured on this fake.");
    }

    public IAsyncEnumerable<GitLabProject> ListContributedByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(ListContributedByUserAsync)} is not configured on this fake.");
    }
}