using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class PackagesComposerClient(IGitLabApiConnection connection) : IPackagesComposerClient
{
    public Task<GitLabFileResponse> GetRepositoryUrlTemplatesForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            // Composer's group-level routes live under the singular "/group/:id" root - unlike every
            // other group resource in this library, which is "/groups/:id". This is GitLab's own route,
            // not a typo: confirmed against the pinned spec.
            GitLabRouteBuilder.Create("group").Segment(groupId).Literal("-").Literal("packages")
                .Literal("composer").Literal("packages").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> ListAllForGroupAsync(GroupId groupId, string sha,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("group").Segment(groupId).Literal("-").Literal("packages")
                .Literal("composer").Literal("p").Escaped(sha).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetPackageVersionsV2ForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("group").Segment(groupId).Literal("-").Literal("packages")
                .Literal("composer").Literal("p2").Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetPackageVersionsForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("group").Segment(groupId).Literal("-").Literal("packages")
                .Literal("composer").Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task CreateAsync(ProjectId projectId, ComposerPackageCreateRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("composer")
                .Build(),
            request ?? new ComposerPackageCreateRequest(),
            GitLabJsonContext.Default.ComposerPackageCreateRequest,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadArchiveAsync(ProjectId projectId, string packageName, string sha,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("composer")
                .Literal("archives").Escaped(packageName).Query("sha", sha).Build(),
            cancellationToken);
    }
}