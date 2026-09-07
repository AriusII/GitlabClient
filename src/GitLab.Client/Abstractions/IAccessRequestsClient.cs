using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Access requests" API area (<c>/projects/:id/access_requests</c> and
///     <c>/groups/:id/access_requests</c>) - the people asking to join, and approving or denying them.
/// </summary>
public interface IAccessRequestsClient
{
    /// <summary>Streams every pending access request on a project that the caller may see.</summary>
    IAsyncEnumerable<GitLabAccessRequest> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Requests access to a project for the authenticated user.</summary>
    Task<GitLabAccessRequest> RequestForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves a project access request, turning the requester into a member. GitLab answers with the
    ///     new <see cref="GitLabMember" /> rather than the access request, which is why the return type
    ///     differs from the other methods here. Omitting <paramref name="request" /> grants GitLab's
    ///     default access level of <c>30</c> (Developer).
    /// </summary>
    Task<GitLabMember> ApproveForProjectAsync(ProjectId projectId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Denies a project access request, removing it. <paramref name="userId" /> is the requester's numeric user ID.</summary>
    Task DenyForProjectAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default);

    /// <summary>Streams every pending access request on a group that the caller may see.</summary>
    IAsyncEnumerable<GitLabAccessRequest> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>Requests access to a group for the authenticated user.</summary>
    Task<GitLabAccessRequest> RequestForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Approves a group access request, turning the requester into a member. As with the project
    ///     counterpart, GitLab answers with the new <see cref="GitLabMember" />.
    /// </summary>
    Task<GitLabMember> ApproveForGroupAsync(GroupId groupId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>Denies a group access request, removing it. <paramref name="userId" /> is the requester's numeric user ID.</summary>
    Task DenyForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default);
}