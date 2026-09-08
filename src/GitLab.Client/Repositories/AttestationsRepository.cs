using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Repositories;

internal sealed class AttestationsRepository(IGitLabApiConnection connection) : IAttestationsRepository
{
    public Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, long attestationIid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("attestations")
                .Segment(attestationIid)
                .Literal("download")
                .Build(),
            cancellationToken);
    }
}