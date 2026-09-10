using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Events" API area (<c>/events</c>, <c>/projects/:id/events</c>,
///     <c>/users/:id/events</c>) - the activity feeds behind "what happened in this project" and "what has
///     this user been doing".
///     <para>
///         These feeds are not an audit log and must not be treated as one. GitLab states that none of the
///         three routes returns epic or merge-request events, activity is subject to a retention limit, and
///         a push beyond the activity limit collapses into a single bulk event with limited commit detail.
///         Auditing belongs to the separate Audit events API.
///     </para>
/// </summary>
public interface IEventsClient
{
    /// <summary>
    ///     Streams the authenticated user's own events (<c>GET /events</c>). This route never returns another
    ///     user's activity, whatever the token's privileges.
    /// </summary>
    /// <param name="options">
    ///     Action, target-type, date-range and sort filters. <see cref="EventListOptions.Scope" /> is honoured
    ///     only here, not by the project or user feeds.
    /// </param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabEvent> ListAsync(EventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the events visible to the caller in one project (<c>GET /projects/:id/events</c>).</summary>
    /// <param name="projectId">The project's numeric id or its namespaced path.</param>
    /// <param name="options">Action, target-type, date-range and sort filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabEvent> ListForProjectAsync(ProjectId projectId, EventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams one user's contribution events (<c>GET /users/:id/events</c>).</summary>
    /// <param name="userId">
    ///     The user's numeric id. GitLab also accepts a username in this position; a username overload is not
    ///     part of this surface yet, matching how <see cref="IUsersClient" /> identifies a user.
    /// </param>
    /// <param name="options">Action, target-type, date-range and sort filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabEvent> ListForUserAsync(long userId, EventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams one user's contribution events (<c>GET /users/:id/events</c>).</summary>
    /// <param name="userId">The user's numeric ID or username.</param>
    /// <param name="options">Action, target-type, date-range and sort filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration, per page.</param>
    IAsyncEnumerable<GitLabEvent> ListForUserAsync(string userId, EventListOptions? options = null,
        CancellationToken cancellationToken = default);
}