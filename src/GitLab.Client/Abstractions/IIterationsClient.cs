using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Iterations" API area (<c>/groups/:id/iterations</c> and
///     <c>/projects/:id/iterations</c>). Read-only: iterations are created and scheduled through iteration
///     cadences, which the REST API does not expose here.
/// </summary>
public interface IIterationsClient
{
    /// <summary>Streams the iterations of a group.</summary>
    IAsyncEnumerable<GitLabIteration> ListForGroupAsync(GroupId groupId, IterationListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the iterations visible to a project. Projects no longer own iterations, so this returns
    ///     the iterations of the project's <b>ancestor groups</b> - it is not a project-scoped collection.
    /// </summary>
    IAsyncEnumerable<GitLabIteration> ListForProjectAsync(ProjectId projectId, IterationListOptions? options = null,
        CancellationToken cancellationToken = default);
}