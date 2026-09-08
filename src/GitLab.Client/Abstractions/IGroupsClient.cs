using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Groups" API area (<c>/groups</c>).</summary>
public interface IGroupsClient
{
    /// <summary>Retrieves one group by numeric ID or namespaced path (<c>GET /groups/:id</c>).</summary>
    Task<GitLabGroup> GetAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Streams every group visible to the caller (<c>GET /groups</c>).</summary>
    IAsyncEnumerable<GitLabGroup> ListAsync(GroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group (<c>POST /groups</c>). Set <see cref="CreateGroupRequest.ParentId" /> to create a
    ///     subgroup; leave it null for a top-level group.
    /// </summary>
    Task<GitLabGroup> CreateAsync(CreateGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a group's settings (<c>PUT /groups/:id</c>). Only the members set on
    ///     <paramref name="request" /> are sent, so nothing is cleared by accident.
    /// </summary>
    Task<GitLabGroup> UpdateAsync(GroupId groupId, UpdateGroupRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the group's avatar (<c>PUT /groups/:id</c> as <c>multipart/form-data</c>). The avatar is
    ///     a binary field on the update endpoint rather than a route of its own, so it is sent on its own
    ///     rather than folded into <see cref="UpdateAsync" />; the file is posted under the form field name
    ///     GitLab expects (<c>avatar</c>) whatever <see cref="GitLabFileUpload.FieldName" /> says.
    /// </summary>
    Task<GitLabGroup> SetAvatarAsync(GroupId groupId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules the group for deletion (<c>DELETE /groups/:id</c>). GitLab answers <c>202 Accepted</c>
    ///     and only marks the group, which is why <see cref="RestoreAsync" /> exists.
    /// </summary>
    Task DeleteAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Cancels a pending deletion (<c>POST /groups/:id/restore</c>).</summary>
    Task RestoreAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Archives the group, making it read-only (<c>POST /groups/:id/archive</c>).</summary>
    Task<GitLabGroup> ArchiveAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Reverses <see cref="ArchiveAsync" /> (<c>POST /groups/:id/unarchive</c>).</summary>
    Task<GitLabGroup> UnarchiveAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Streams the group's direct children (<c>GET /groups/:id/subgroups</c>).</summary>
    IAsyncEnumerable<GitLabGroup> ListSubgroupsAsync(GroupId groupId, GroupHierarchyListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the group's whole subtree, at any depth (<c>GET /groups/:id/descendant_groups</c>) - the
    ///     recursive counterpart of <see cref="ListSubgroupsAsync" />.
    /// </summary>
    IAsyncEnumerable<GitLabGroup> ListDescendantGroupsAsync(GroupId groupId, GroupHierarchyListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the groups this group has been shared with (<c>GET /groups/:id/groups/shared</c>).</summary>
    IAsyncEnumerable<GitLabGroup> ListSharedGroupsAsync(GroupId groupId, SharedGroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the groups invited into this one (<c>GET /groups/:id/invited_groups</c>).</summary>
    IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(GroupId groupId, InvitedGroupListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the projects the group owns (<c>GET /groups/:id/projects</c>).</summary>
    IAsyncEnumerable<GitLabProject> ListProjectsAsync(GroupId groupId, GroupProjectListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the projects shared into the group (<c>GET /groups/:id/projects/shared</c>).</summary>
    IAsyncEnumerable<GitLabProject> ListSharedProjectsAsync(GroupId groupId,
        GroupSharedProjectListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves an existing project into this group (<c>POST /groups/:id/projects/:project_id</c>) and
    ///     answers with the receiving group.
    /// </summary>
    Task<GitLabGroup> TransferProjectAsync(GroupId groupId, ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Withdraws a project that was shared with the group
    ///     (<c>DELETE /groups/:id/shared_projects/:project_id</c>). The project itself is untouched.
    /// </summary>
    Task RemoveSharedProjectAsync(GroupId groupId, ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Shares the group with another group (<c>POST /groups/:id/share</c>).</summary>
    Task<GitLabGroup> ShareAsync(GroupId groupId, ShareGroupRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Reverses <see cref="ShareAsync" /> (<c>DELETE /groups/:id/share/:group_id</c>).</summary>
    /// <param name="groupId">The group that granted the share.</param>
    /// <param name="sharedWithGroupId">The group the share was granted to.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task UnshareAsync(GroupId groupId, long sharedWithGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves the group under a new parent (<c>POST /groups/:id/transfer</c>). Leaving
    ///     <see cref="TransferGroupRequest.GroupId" /> null promotes it to a top-level group.
    /// </summary>
    Task TransferAsync(GroupId groupId, TransferGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the groups this one may be moved under (<c>GET /groups/:id/transfer_locations</c>) - the
    ///     candidates for <see cref="TransferAsync" />.
    /// </summary>
    IAsyncEnumerable<GitLabGroup> ListTransferLocationsAsync(GroupId groupId,
        GroupTransferLocationListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Moves the group into another organization (<c>POST /groups/:id/transfer_to_organization</c>).
    ///     Distinct from <see cref="TransferAsync" />, which moves it within the same organization.
    /// </summary>
    Task<GitLabGroup> TransferToOrganizationAsync(GroupId groupId, TransferGroupToOrganizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams issues across every project in the group and its subgroups
    ///     (<c>GET /groups/:id/issues</c>).
    /// </summary>
    IAsyncEnumerable<GitLabIssue> ListIssuesAsync(GroupId groupId, GroupIssueListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the group's issues by state (<c>GET /groups/:id/issues_statistics</c>), over exactly the
    ///     issues <paramref name="options" /> selects.
    /// </summary>
    Task<GitLabGroupIssueStatistics> GetIssueStatisticsAsync(GroupId groupId,
        GroupIssueStatisticsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users consuming a billable seat in a top-level group
    ///     (<c>GET /groups/:id/billable_members</c>). Removing one, and listing the memberships behind a
    ///     seat, live on the Members API.
    /// </summary>
    IAsyncEnumerable<GitLabBillableMember> ListBillableMembersAsync(GroupId groupId,
        GroupBillableMemberListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users this group provisioned (<c>GET /groups/:id/provisioned_users</c>) - accounts
    ///     created by the group's own SAML or SCIM integration.
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListProvisionedUsersAsync(GroupId groupId, GroupUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the users with a SAML identity in this group (<c>GET /groups/:id/saml_users</c>), which
    ///     is a wider set than <see cref="ListProvisionedUsersAsync" />.
    /// </summary>
    IAsyncEnumerable<GitLabUser> ListSamlUsersAsync(GroupId groupId, GroupUserListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the files attached to the group's markdown (<c>GET /groups/:id/uploads</c>).</summary>
    IAsyncEnumerable<GitLabGroupUpload>
        ListUploadsAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a file to the group (<c>POST /groups/:id/uploads</c>) and answers with the markdown that
    ///     embeds it. The stream on <paramref name="file" /> is read but not disposed.
    /// </summary>
    Task<GitLabGroupUploadedFile> UploadFileAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks the GitLab Workhorse layer to authorize a direct file upload
    ///     (<c>POST /groups/:id/uploads/authorize</c>) before the actual upload request. GitLab answers with
    ///     Workhorse-internal routing details this client has no use for, so the response body is discarded;
    ///     callers only need to know the call succeeded.
    /// </summary>
    Task AuthorizeUploadAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one upload by its ID (<c>GET /groups/:id/uploads/:upload_id</c>). The caller owns the
    ///     returned response and must dispose it.
    /// </summary>
    Task<GitLabFileResponse> DownloadUploadAsync(GroupId groupId, long uploadId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one upload by the secret and file name from its markdown URL
    ///     (<c>GET /groups/:id/uploads/:secret/:filename</c>). The caller owns the returned response and
    ///     must dispose it.
    /// </summary>
    Task<GitLabFileResponse> DownloadUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one upload by its ID (<c>DELETE /groups/:id/uploads/:upload_id</c>).</summary>
    Task DeleteUploadAsync(GroupId groupId, long uploadId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes one upload by the secret and file name from its markdown URL
    ///     (<c>DELETE /groups/:id/uploads/:secret/:filename</c>).
    /// </summary>
    Task DeleteUploadBySecretAsync(GroupId groupId, string secret, string filename,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads the CSV of placeholder users still awaiting reassignment after an import
    ///     (<c>GET /groups/:id/placeholder_reassignments</c>). The body is a CSV file, not JSON, so the
    ///     caller owns the returned response and must dispose it.
    /// </summary>
    Task<GitLabFileResponse> DownloadPlaceholderReassignmentsAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reassigns placeholder users created by an import, from an uploaded CSV file
    ///     (<c>POST /groups/:id/placeholder_reassignments</c>). The CSV's shape matches what
    ///     <see cref="DownloadPlaceholderReassignmentsAsync" /> downloads. The file's stream is read but not
    ///     disposed.
    /// </summary>
    Task ReassignPlaceholdersAsync(GroupId groupId, GitLabFileUpload file,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authorizes GitLab Workhorse to accept the reassignment CSV upload ahead of
    ///     <see cref="ReassignPlaceholdersAsync" /> (<c>POST /groups/:id/placeholder_reassignments/authorize</c>).
    /// </summary>
    Task AuthorizePlaceholderReassignmentsAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves one entry from the group's audit log (<c>GET /groups/:id/audit_events/:audit_event_id</c>).
    ///     Restricted to group Owners and administrators.
    /// </summary>
    Task<GitLabGroupAuditEvent> GetAuditEventAsync(GroupId groupId, long auditEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the group's security settings, such as secret push protection
    ///     (<c>PUT /groups/:id/security_settings</c>). Requires the Security Manager, Maintainer or Owner
    ///     role.
    /// </summary>
    Task UpdateSecuritySettingsAsync(GroupId groupId, UpdateGroupSecuritySettingsRequest request,
        CancellationToken cancellationToken = default);
}