using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class GroupsClient(IGitLabApiConnection connection) : IGroupsClient
{
    /// <summary>
    ///     The form field GitLab's group update endpoint reads the avatar from. Forced here rather than
    ///     trusted from the caller's <see cref="GitLabFileUpload" />, whose default field name is "file" -
    ///     sending it under that name is accepted with a 200 and silently ignored.
    /// </summary>
    private const string AvatarFieldName = "avatar";

    public Task<GitLabGroup> GetAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Build(),
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task<GitLabGroup> GetAsync(GroupId groupId, GroupGetOptions options,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListAsync(GroupListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public Task<GitLabGroup> CreateAsync(CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostMultipartFormAsync(
            GitLabRouteBuilder.Create("groups").Build(),
            request,
            GitLabJsonContext.Default.CreateGroupRequest,
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task<GitLabGroup> UpdateAsync(GroupId groupId, UpdateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutMultipartFormAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupRequest,
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task<GitLabGroup> SetAvatarAsync(GroupId groupId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Build(),
            avatar with { FieldName = AvatarFieldName },
            null,
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task DeleteAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Build(),
            cancellationToken);
    }

    public Task RestoreAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("restore").Build(),
            cancellationToken);
    }

    public Task<GitLabGroup> ArchiveAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("archive").Build(),
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task<GitLabGroup> UnarchiveAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("unarchive").Build(),
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListSubgroupsAsync(GroupId groupId,
        GroupHierarchyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("subgroups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListDescendantGroupsAsync(GroupId groupId,
        GroupHierarchyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("descendant_groups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListSharedGroupsAsync(GroupId groupId,
        SharedGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("groups").Literal("shared")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(GroupId groupId,
        InvitedGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("invited_groups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListProjectsAsync(GroupId groupId,
        GroupProjectListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListSharedProjectsAsync(GroupId groupId,
        GroupSharedProjectListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("projects").Literal("shared")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public Task<GitLabGroup> TransferProjectAsync(GroupId groupId, ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("projects").Segment(projectId).Build(),
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task RemoveSharedProjectAsync(GroupId groupId, ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("shared_projects").Segment(projectId).Build(),
            cancellationToken);
    }

    public Task<GitLabGroup> ShareAsync(GroupId groupId, ShareGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("share").Build(),
            request,
            GitLabJsonContext.Default.ShareGroupRequest,
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public Task UnshareAsync(GroupId groupId, long sharedWithGroupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("share").Segment(sharedWithGroupId).Build(),
            cancellationToken);
    }

    public Task TransferAsync(GroupId groupId, TransferGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("transfer").Build(),
            request,
            GitLabJsonContext.Default.TransferGroupRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListTransferLocationsAsync(GroupId groupId,
        GroupTransferLocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("transfer_locations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public Task<GitLabGroup> TransferToOrganizationAsync(GroupId groupId,
        TransferGroupToOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("transfer_to_organization").Build(),
            request,
            GitLabJsonContext.Default.TransferGroupToOrganizationRequest,
            GitLabJsonContext.Default.GitLabGroup,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIssue> ListIssuesAsync(GroupId groupId, GroupIssueListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("issues")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabIssueArray,
            cancellationToken);
    }

    public Task<GitLabGroupIssueStatistics> GetIssueStatisticsAsync(GroupId groupId,
        GroupIssueStatisticsOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("issues_statistics")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupIssueStatistics,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBillableMember> ListBillableMembersAsync(GroupId groupId,
        GroupBillableMemberListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("billable_members")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabBillableMemberArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListProvisionedUsersAsync(GroupId groupId,
        GroupUserListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("provisioned_users")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListSamlUsersAsync(GroupId groupId, GroupUserListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("saml_users")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupUpload> ListUploadsAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads").Build(),
            GitLabJsonContext.Default.GitLabGroupUploadArray,
            cancellationToken);
    }

    public Task<GitLabGroupUploadedFile> UploadFileAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads").Build(),
            file,
            null,
            GitLabJsonContext.Default.GitLabGroupUploadedFile,
            cancellationToken);
    }

    public Task AuthorizeUploadAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadUploadAsync(GroupId groupId, long uploadId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads").Segment(uploadId).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads")
                .Escaped(secret).Escaped(filename).Build(),
            cancellationToken);
    }

    public Task DeleteUploadAsync(GroupId groupId, long uploadId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads").Segment(uploadId).Build(),
            cancellationToken);
    }

    public Task DeleteUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("uploads")
                .Escaped(secret).Escaped(filename).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadPlaceholderReassignmentsAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("placeholder_reassignments").Build(),
            cancellationToken);
    }

    public Task ReassignPlaceholdersAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("placeholder_reassignments").Build(),
            file,
            null,
            cancellationToken);
    }

    public Task AuthorizePlaceholderReassignmentsAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("placeholder_reassignments")
                .Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabGroupAuditEvent> GetAuditEventAsync(GroupId groupId, long auditEventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("audit_events").Segment(auditEventId)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupAuditEvent,
            cancellationToken);
    }

    public Task UpdateSecuritySettingsAsync(GroupId groupId, UpdateGroupSecuritySettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("security_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupSecuritySettingsRequest,
            cancellationToken);
    }
}