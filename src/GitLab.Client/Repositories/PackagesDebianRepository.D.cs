using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

/// <summary>Part D: the paired upload for <see cref="AuthorizePackageUploadAsync" />.</summary>
internal sealed partial class PackagesDebianRepository
{
    public Task UploadPackageFileAsync(ProjectId projectId, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("debian")
                .Escaped(fileName).Build(),
            file,
            null,
            cancellationToken);
    }
}