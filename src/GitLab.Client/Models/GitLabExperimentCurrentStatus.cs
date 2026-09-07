namespace GitLab.Client.Models;

/// <summary>
///     Rollout state of one <see cref="GitLabExperiment" />, embedded as
///     <see cref="GitLabExperiment.CurrentStatus" />.
/// </summary>
public sealed record GitLabExperimentCurrentStatus
{
    /// <summary>
    ///     The experiment's overall Flipper state - <c>on</c>, <c>off</c> or <c>conditional</c>. The spec
    ///     types it as a bare string with no enumeration, so it stays a string here.
    /// </summary>
    public string? State { get; init; }

    /// <summary>The single rollout gate GitLab's spec associates with this status.</summary>
    public GitLabFeatureGate? Gates { get; init; }
}