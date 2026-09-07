using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Iterations, sitting between the public <c>IIterationsClient</c>
///     controller and <c>IIterationsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IIterationsService
{
    IAsyncEnumerable<GitLabIteration> ListForGroupAsync(GroupId groupId, IterationListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIteration> ListForProjectAsync(ProjectId projectId, IterationListOptions? options = null,
        CancellationToken cancellationToken = default);
}