using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Endpoints;

internal sealed class PackagesCargoClient(IGitLabApiConnection connection) : IPackagesCargoClient
{
    public Task<GitLabFileResponse> GetSparseIndexForOneCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Literal("1").Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSparseIndexForTwoCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Literal("2").Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSparseIndexForThreeCharacterNameAsync(ProjectId projectId, string firstChar,
        string packageName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Literal("3").Escaped(firstChar).Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSparseIndexAsync(ProjectId projectId, string prefix1, string prefix2,
        string packageName, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Escaped(prefix1).Escaped(prefix2).Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetConfigAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Literal("config.json").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadCrateAsync(ProjectId projectId, string packageName,
        string packageVersion, CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("cargo")
                .Escaped(packageName).Escaped(packageVersion).Literal("download").Build(),
            cancellationToken);
    }
}