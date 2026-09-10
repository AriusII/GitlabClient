using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class AccessRequestsClient(IGitLabApiConnection connection) : IAccessRequestsClient
{
    public IAsyncEnumerable<GitLabAccessRequest> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("access_requests").Build(),
            GitLabJsonContext.Default.GitLabAccessRequestArray,
            cancellationToken);
    }

    public Task<GitLabAccessRequest> RequestForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        // GitLab derives the requester from the credential, so this POST carries no body but still
        // answers with the created access request - the empty-bodied action overload, not PostAsync<TRequest, TResponse>.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("access_requests").Build(),
            GitLabJsonContext.Default.GitLabAccessRequest,
            cancellationToken);
    }

    public Task<GitLabMember> ApproveForProjectAsync(ProjectId projectId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("access_requests").Segment(userId)
                .Literal("approve").Build(),
            request ?? new ApproveAccessRequestRequest(),
            GitLabJsonContext.Default.ApproveAccessRequestRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task DenyForProjectAsync(ProjectId projectId, long userId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("access_requests").Segment(userId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAccessRequest> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("access_requests").Build(),
            GitLabJsonContext.Default.GitLabAccessRequestArray,
            cancellationToken);
    }

    public Task<GitLabAccessRequest> RequestForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("access_requests").Build(),
            GitLabJsonContext.Default.GitLabAccessRequest,
            cancellationToken);
    }

    public Task<GitLabMember> ApproveForGroupAsync(GroupId groupId, long userId,
        ApproveAccessRequestRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("access_requests").Segment(userId)
                .Literal("approve").Build(),
            request ?? new ApproveAccessRequestRequest(),
            GitLabJsonContext.Default.ApproveAccessRequestRequest,
            GitLabJsonContext.Default.GitLabMember,
            cancellationToken);
    }

    public Task DenyForGroupAsync(GroupId groupId, long userId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("access_requests").Segment(userId).Build(),
            cancellationToken);
    }
}