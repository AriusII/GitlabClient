using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Groups resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IGroupsService), typeof(IGroupsClient))]
internal interface IGroupsRepository
{
    Task<GitLabGroup> GetAsync(GroupId groupId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListAsync(GroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabGroup> CreateAsync(CreateGroupRequest request, CancellationToken cancellationToken = default);

    Task<GitLabGroup> UpdateAsync(GroupId groupId, UpdateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGroup> SetAvatarAsync(GroupId groupId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task RestoreAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroup> ArchiveAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroup> UnarchiveAsync(GroupId groupId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListSubgroupsAsync(GroupId groupId, GroupHierarchyListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListDescendantGroupsAsync(GroupId groupId, GroupHierarchyListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListSharedGroupsAsync(GroupId groupId, SharedGroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(GroupId groupId, InvitedGroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListProjectsAsync(GroupId groupId, GroupProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabProject> ListSharedProjectsAsync(GroupId groupId,
        GroupSharedProjectListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabGroup> TransferProjectAsync(GroupId groupId, ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task RemoveSharedProjectAsync(GroupId groupId, ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabGroup> ShareAsync(GroupId groupId, ShareGroupRequest request,
        CancellationToken cancellationToken = default);

    Task UnshareAsync(GroupId groupId, long sharedWithGroupId, CancellationToken cancellationToken = default);

    Task TransferAsync(GroupId groupId, TransferGroupRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroup> ListTransferLocationsAsync(GroupId groupId,
        GroupTransferLocationListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabGroup> TransferToOrganizationAsync(GroupId groupId, TransferGroupToOrganizationRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIssue> ListIssuesAsync(GroupId groupId, GroupIssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupIssueStatistics> GetIssueStatisticsAsync(GroupId groupId,
        GroupIssueStatisticsOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabBillableMember> ListBillableMembersAsync(GroupId groupId,
        GroupBillableMemberListOptions? options = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListProvisionedUsersAsync(GroupId groupId, GroupUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabUser> ListSamlUsersAsync(GroupId groupId, GroupUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupUpload>
        ListUploadsAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroupUploadedFile> UploadFileAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadUploadAsync(GroupId groupId, long uploadId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default);

    Task DeleteUploadAsync(GroupId groupId, long uploadId, CancellationToken cancellationToken = default);

    Task DeleteUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadPlaceholderReassignmentsAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task ReassignPlaceholdersAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    Task AuthorizePlaceholderReassignmentsAsync(GroupId groupId, CancellationToken cancellationToken = default);

    Task<GitLabGroupAuditEvent> GetAuditEventAsync(GroupId groupId, long auditEventId,
        CancellationToken cancellationToken = default);

    Task UpdateSecuritySettingsAsync(GroupId groupId, UpdateGroupSecuritySettingsRequest request,
        CancellationToken cancellationToken = default);
}