using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ReleasesRepository(IGitLabApiConnection connection) : IReleasesRepository
{
    public IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Build(),
            GitLabJsonContext.Default.GitLabReleaseArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabRelease> ListForGroupAsync(GroupId groupId, GroupReleaseListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal("releases")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabReleaseArray,
            cancellationToken);
    }

    public Task<GitLabRelease> GetAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .Build(),
            GitLabJsonContext.Default.GitLabRelease,
            cancellationToken);
    }

    public Task<GitLabRelease> CreateAsync(ProjectId projectId, CreateReleaseRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateReleaseRequest,
            GitLabJsonContext.Default.GitLabRelease,
            cancellationToken);
    }

    public Task<GitLabRelease> UpdateAsync(ProjectId projectId, string tagName, UpdateReleaseRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateReleaseRequest,
            GitLabJsonContext.Default.GitLabRelease,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabRelease> GenerateEvidenceAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .Literal("evidence")
                .Build(),
            GitLabJsonContext.Default.GitLabRelease,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabReleaseLink> ListLinksAsync(ProjectId projectId, string tagName,
        ReleaseLinkListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            LinksRoute(projectId, tagName)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabReleaseLinkArray,
            cancellationToken);
    }

    public Task<GitLabReleaseLink> GetLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            LinksRoute(projectId, tagName).Segment(linkId).Build(),
            GitLabJsonContext.Default.GitLabReleaseLink,
            cancellationToken);
    }

    public Task<GitLabReleaseLink> CreateLinkAsync(ProjectId projectId, string tagName,
        CreateReleaseLinkRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            LinksRoute(projectId, tagName).Build(),
            request,
            GitLabJsonContext.Default.CreateReleaseLinkRequest,
            GitLabJsonContext.Default.GitLabReleaseLink,
            cancellationToken);
    }

    public Task<GitLabReleaseLink> UpdateLinkAsync(ProjectId projectId, string tagName, long linkId,
        UpdateReleaseLinkRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            LinksRoute(projectId, tagName).Segment(linkId).Build(),
            request,
            GitLabJsonContext.Default.UpdateReleaseLinkRequest,
            GitLabJsonContext.Default.GitLabReleaseLink,
            cancellationToken);
    }

    public Task DeleteLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            LinksRoute(projectId, tagName).Segment(linkId).Build(),
            cancellationToken);
    }

    private static GitLabRouteBuilder LinksRoute(ProjectId projectId, string tagName)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("releases")
            .Escaped(tagName)
            .Literal("assets")
            .Literal("links");
    }
}