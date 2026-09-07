using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Events, sitting between the public <c>IEventsClient</c>
///     controller and <c>IEventsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IEventsService
{
    IAsyncEnumerable<GitLabEvent> ListAsync(EventListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabEvent> ListForProjectAsync(ProjectId projectId, EventListOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabEvent> ListForUserAsync(long userId, EventListOptions? options = null,
        CancellationToken cancellationToken = default);
}