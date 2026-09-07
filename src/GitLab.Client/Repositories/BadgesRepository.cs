using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class BadgesRepository(IGitLabApiConnection connection) : IBadgesRepository
{
    private const string BadgesSegment = "badges";

    /// <summary>
    ///     GitLab's preview endpoint sits at <c>.../badges/render</c>, the same position a numeric
    ///     <c>badge_id</c> occupies. It is appended with <see cref="GitLabRouteBuilder.Literal" /> so no
    ///     caller-supplied value can ever land in that slot.
    /// </summary>
    private const string RenderSegment = "render";

    private const string LinkUrlParameter = "link_url";
    private const string ImageUrlParameter = "image_url";

    public IAsyncEnumerable<GitLabBadge> ListForProjectAsync(ProjectId projectId, string? name = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Query("name", name)
                .Build(),
            GitLabJsonContext.Default.GitLabBadgeArray,
            cancellationToken);
    }

    public Task<GitLabBadge> GetForProjectAsync(ProjectId projectId, long badgeId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task<GitLabBadge> CreateForProjectAsync(ProjectId projectId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateBadgeRequest,
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task<GitLabBadge> UpdateForProjectAsync(ProjectId projectId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateBadgeRequest,
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, long badgeId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabBadgePreview> PreviewForProjectAsync(ProjectId projectId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(BadgesSegment)
                .Literal(RenderSegment)
                .Query(LinkUrlParameter, linkUrl)
                .Query(ImageUrlParameter, imageUrl)
                .Build(),
            GitLabJsonContext.Default.GitLabBadgePreview,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabBadge> ListForGroupAsync(GroupId groupId, string? name = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Query("name", name)
                .Build(),
            GitLabJsonContext.Default.GitLabBadgeArray,
            cancellationToken);
    }

    public Task<GitLabBadge> GetForGroupAsync(GroupId groupId, long badgeId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task<GitLabBadge> CreateForGroupAsync(GroupId groupId, CreateBadgeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateBadgeRequest,
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task<GitLabBadge> UpdateForGroupAsync(GroupId groupId, long badgeId, UpdateBadgeRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateBadgeRequest,
            GitLabJsonContext.Default.GitLabBadge,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, long badgeId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Segment(badgeId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabBadgePreview> PreviewForGroupAsync(GroupId groupId, string linkUrl, string imageUrl,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(BadgesSegment)
                .Literal(RenderSegment)
                .Query(LinkUrlParameter, linkUrl)
                .Query(ImageUrlParameter, imageUrl)
                .Build(),
            GitLabJsonContext.Default.GitLabBadgePreview,
            cancellationToken);
    }
}