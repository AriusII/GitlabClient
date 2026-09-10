using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ReleasesClient(IGitLabApiConnection connection) : IReleasesClient
{
    public IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return ListAsync(projectId, null, cancellationToken);
    }

    public IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, ReleaseListOptions? options,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .QueryFrom(options)
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
        return GetAsync(projectId, tagName, null, cancellationToken);
    }

    public Task<GitLabRelease> GetAsync(ProjectId projectId, string tagName, ReleaseGetOptions? options,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .QueryFrom(options)
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

    /// <summary>
    ///     Retrieves the project's latest release without knowing its tag name in advance. The spec declares
    ///     no response schema for this permalink route, so the raw body is returned rather than an invented
    ///     shape.
    /// </summary>
    public Task<GitLabFileResponse> GetLatestReleaseAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            LatestReleasePermalinkRoute(projectId).Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Same permalink as <see cref="GetLatestReleaseAsync" />, with a caller path appended after
    ///     resolving to the latest release - for example a downloads path - so the caller never needs the
    ///     tag name up front. <paramref name="suffixPath" /> commonly contains <c>/</c>, so it is escaped
    ///     rather than appended as a literal.
    /// </summary>
    public Task<GitLabFileResponse> GetLatestReleaseSuffixPathAsync(ProjectId projectId, string suffixPath,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            LatestReleasePermalinkRoute(projectId).Escaped(suffixPath).Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Downloads one asset file attached to a release by the direct asset path recorded on its link. The
    ///     spec declares no response schema - the body is whatever content the asset link points at.
    /// </summary>
    public Task<GitLabFileResponse> DownloadReleaseAssetAsync(ProjectId projectId, string tagName,
        string directAssetPath, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("releases")
                .Escaped(tagName)
                .Literal("downloads")
                .Escaped(directAssetPath)
                .Build(),
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

    private static GitLabRouteBuilder LatestReleasePermalinkRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("releases")
            .Literal("permalink")
            .Literal("latest");
    }
}