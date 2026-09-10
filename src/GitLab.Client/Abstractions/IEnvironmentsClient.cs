using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Environments" API area (<c>/projects/:id/environments</c>).</summary>
public interface IEnvironmentsClient
{
    /// <summary>Streams every environment configured on the project.</summary>
    IAsyncEnumerable<GitLabEnvironment> ListAsync(ProjectId projectId, EnvironmentListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one environment's details.</summary>
    Task<GitLabEnvironment> GetAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new environment.</summary>
    Task<GitLabEnvironment> CreateAsync(ProjectId projectId, CreateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an environment's external URL, tier, or other configuration.</summary>
    Task<GitLabEnvironment> UpdateAsync(ProjectId projectId, long environmentId, UpdateEnvironmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Stops the environment, running its <c>on_stop</c> action if one is defined.</summary>
    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Stops the environment, optionally forcing it even when no stop action is defined.</summary>
    Task<GitLabEnvironment> StopAsync(ProjectId projectId, long environmentId, bool force,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an environment (<c>DELETE /projects/:id/environments/:environment_id</c>).</summary>
    Task DeleteAsync(ProjectId projectId, long environmentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stops every environment last modified or deployed to before
    ///     <see cref="StopStaleEnvironmentsRequest.Before" />, excluding protected environments
    ///     (<c>POST /projects/:id/environments/stop_stale</c>).
    /// </summary>
    Task StopStaleAsync(ProjectId projectId, StopStaleEnvironmentsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Schedules stopped review apps for deletion, which GitLab performs a week later
    ///     (<c>DELETE /projects/:id/environments/review_apps</c>).
    ///     <para>
    ///         GitLab defaults <see cref="ReviewAppDeletionOptions.DryRun" /> to <c>true</c>. The typed result
    ///         exposes both the dry-run preview and entries GitLab could not schedule, so callers can inspect the
    ///         effect before opting into <c>dry_run=false</c>.
    ///     </para>
    /// </summary>
    Task<GitLabReviewAppDeletionResult> DeleteReviewAppsAsync(ProjectId projectId,
        ReviewAppDeletionOptions? options = null,
        CancellationToken cancellationToken = default);
}