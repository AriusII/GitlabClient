using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class AvatarsClient(IGitLabApiConnection connection) : IAvatarsClient
{
    public Task<GitLabAvatar> GetForEmailAsync(string email, int? size = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("avatar").Query("email", email).Query("size", size).Build(),
            GitLabJsonContext.Default.GitLabAvatar,
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("avatar").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("avatar").Build(),
            cancellationToken);
    }
}