using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class InvitationsClient(IGitLabApiConnection connection) : IInvitationsClient
{
    public IAsyncEnumerable<GitLabInvitation> ListForProjectAsync(ProjectId projectId,
        InvitationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("invitations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabInvitationArray,
            cancellationToken);
    }

    public Task<GitLabInvitation> CreateForProjectAsync(ProjectId projectId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("invitations").Build(),
            request,
            GitLabJsonContext.Default.CreateInvitationRequest,
            GitLabJsonContext.Default.GitLabInvitation,
            cancellationToken);
    }

    public Task<GitLabInvitation> UpdateForProjectAsync(ProjectId projectId, string email,
        UpdateInvitationRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("invitations").Escaped(email).Build(),
            request,
            GitLabJsonContext.Default.UpdateInvitationRequest,
            GitLabJsonContext.Default.GitLabInvitation,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, string email, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("invitations").Escaped(email).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabInvitation> ListForGroupAsync(GroupId groupId, InvitationListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("invitations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabInvitationArray,
            cancellationToken);
    }

    public Task<GitLabInvitation> CreateForGroupAsync(GroupId groupId, CreateInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("invitations").Build(),
            request,
            GitLabJsonContext.Default.CreateInvitationRequest,
            GitLabJsonContext.Default.GitLabInvitation,
            cancellationToken);
    }

    public Task<GitLabInvitation> UpdateForGroupAsync(GroupId groupId, string email, UpdateInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("invitations").Escaped(email).Build(),
            request,
            GitLabJsonContext.Default.UpdateInvitationRequest,
            GitLabJsonContext.Default.GitLabInvitation,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, string email, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("invitations").Escaped(email).Build(),
            cancellationToken);
    }
}