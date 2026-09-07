namespace GitLab.Client.Models;

/// <summary>
///     Instance-wide dependency management settings, nested in
///     <see cref="UpdateApplicationSettingsRequest.DependencyManagementSettings" />.
/// </summary>
public sealed record GitLabDependencyManagementSettings
{
    /// <summary>
    ///     Maximum number of dependency management security-update scheduler jobs that run
    ///     concurrently across the Sidekiq fleet.
    /// </summary>
    public int? SecurityUpdateSchedulerMaxConcurrency { get; init; }
}