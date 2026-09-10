using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Endpoints;

internal sealed class PackagesRubyGemsClient(IGitLabApiConnection connection) : IPackagesRubyGemsClient
{
    public Task<GitLabFileResponse> GetSpecIndexAsync(ProjectId projectId, GitLabRubyGemsSpecIndexFile file,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal(file.ToRouteValue()).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetGemspecAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal("quick").Literal("Marshal.4.8").Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadGemAsync(ProjectId projectId, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal("gems").Escaped(fileName).Build(),
            cancellationToken);
    }

    public Task AuthorizeGemUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal("api").Literal("v1").Literal("gems").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task UploadGemAsync(ProjectId projectId, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal("api").Literal("v1").Literal("gems").Build(),
            file,
            null,
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetDependenciesAsync(ProjectId projectId,
        RubyGemsDependencyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("rubygems")
                .Literal("api").Literal("v1").Literal("dependencies").QueryFrom(options).Build(),
            cancellationToken);
    }
}