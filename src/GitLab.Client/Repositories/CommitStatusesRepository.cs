using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class CommitStatusesRepository(IGitLabApiConnection connection) : ICommitStatusesRepository
{
    // The read and write routes share no prefix: the sha sits mid-path under repository/commits for the
    // read, and at the very end of a flat statuses route for the write. They are built independently on
    // purpose - factoring a "common" prefix out of them would be wrong.
    public IAsyncEnumerable<GitLabCommitStatus> ListAsync(ProjectId projectId, string sha,
        CommitStatusListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("repository")
                .Literal("commits")
                .Escaped(sha)
                .Literal("statuses")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabCommitStatusArray,
            cancellationToken);
    }

    public Task<GitLabCommitStatus> CreateAsync(ProjectId projectId, string sha, CreateCommitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("statuses")
                .Escaped(sha)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateCommitStatusRequest,
            GitLabJsonContext.Default.GitLabCommitStatus,
            cancellationToken);
    }
}