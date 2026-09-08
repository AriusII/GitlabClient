using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Project mirrors" API area (<c>/projects/:id/mirror/pull</c>) - a project's pull
///     mirror: its configuration, its current sync status, and starting an update of it.
///     <para>
///         This is distinct from <see cref="IRemoteMirrorsClient" />, which manages the (possibly several)
///         <em>push</em> mirrors under <c>/projects/:id/remote_mirrors</c>. A project has at most one pull
///         mirror, configured here.
///     </para>
/// </summary>
public interface IProjectMirrorsClient
{
    /// <summary>Retrieves the project's pull mirror configuration and status.</summary>
    Task<GitLabPullMirror> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts the pull mirroring process. GitLab declares no response body for this route, so the call
    ///     completes once the update has been queued. Pass <see cref="TriggerPullMirrorRequest.Force" /> to
    ///     reset a hard-failed mirror and retry; <paramref name="request" /> can otherwise be omitted
    ///     entirely for the common "just sync now" case.
    ///     <para>
    ///         GitLab also accepts a GitHub pull-request-shaped payload on this same route
    ///         (<see cref="TriggerPullMirrorRequest.PullRequest" />) to relay a GitHub webhook and keep a
    ///         GitHub-sourced pull mirror's merge requests in sync without waiting for the next scheduled
    ///         pull.
    ///     </para>
    /// </summary>
    Task StartAsync(ProjectId projectId, TriggerPullMirrorRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the project's pull mirroring settings (introduced in GitLab 17.5). Only the fields set on
    ///     <paramref name="request" /> are sent.
    /// </summary>
    Task<GitLabPullMirror> UpdateAsync(ProjectId projectId, UpdatePullMirrorRequest request,
        CancellationToken cancellationToken = default);
}