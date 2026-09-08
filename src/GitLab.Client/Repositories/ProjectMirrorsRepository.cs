using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ProjectMirrorsRepository(IGitLabApiConnection connection) : IProjectMirrorsRepository
{
    public Task<GitLabPullMirror> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            MirrorPullRoute(projectId).Build(),
            GitLabJsonContext.Default.GitLabPullMirror,
            cancellationToken);
    }

    /// <summary>
    ///     Starts the pull mirroring process. GitLab declares no response schema for this route, so this goes
    ///     through one of the body-less-response overloads of <see cref="IGitLabApiConnection" /> rather than
    ///     an invented response shape: with no <paramref name="request" />, the plain
    ///     <see cref="IGitLabApiConnection.PostAsync(Uri, CancellationToken)" />; with one, the
    ///     request-in/no-body-out <c>PostAsync&lt;TRequest&gt;</c> overload.
    /// </summary>
    public Task StartAsync(ProjectId projectId, TriggerPullMirrorRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        Uri uri = MirrorPullRoute(projectId).Build();

        return request is null
            ? connection.PostAsync(uri, cancellationToken)
            : connection.PostAsync(uri, request, GitLabJsonContext.Default.TriggerPullMirrorRequest,
                cancellationToken);
    }

    public Task<GitLabPullMirror> UpdateAsync(ProjectId projectId, UpdatePullMirrorRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            MirrorPullRoute(projectId).Build(),
            request,
            GitLabJsonContext.Default.UpdatePullMirrorRequest,
            GitLabJsonContext.Default.GitLabPullMirror,
            cancellationToken);
    }

    private static GitLabRouteBuilder MirrorPullRoute(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("mirror")
            .Literal("pull");
    }
}