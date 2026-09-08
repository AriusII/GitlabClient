using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Environments resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IEnvironmentsService), typeof(IEnvironmentsClient))]
internal interface IEnvironmentsRepository
{
    IAsyncEnumerable<GitLabEnvironment> ListAsync(ProjectId projectId, EnvironmentListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> GetAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> CreateAsync(ProjectId projectId, CreateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> UpdateAsync(ProjectId projectId, long environmentId, UpdateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     An overload rather than an optional <c>force</c> parameter: inserting one before the
    ///     <see cref="CancellationToken" /> would silently break every existing three-argument call site.
    /// </summary>
    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId, bool force,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long environmentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     <c>POST /projects/:id/environments/stop_stale</c>. GitLab's own verb, not an update.
    /// </summary>
    Task StopStaleAsync(ProjectId projectId, StopStaleEnvironmentsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     <c>DELETE /projects/:id/environments/review_apps</c>. GitLab answers with the entries it
    ///     scheduled, but this method discards that body rather than surfacing it as a typed result.
    /// </summary>
    Task DeleteReviewAppsAsync(ProjectId projectId, ReviewAppDeletionOptions? options = null,
        CancellationToken cancellationToken = default);
}