namespace GitLab.Client.Models;

/// <summary>
///     One A/B experiment configured on the GitLab instance (<c>GET /experiments</c>). Internal-use API:
///     GitLab documents it as unusable with anonymous or unauthenticated callers.
/// </summary>
public sealed record GitLabExperiment
{
    /// <summary>The experiment's unique key, as passed to <c>experiment_name</c>/<c>name</c> elsewhere in this area.</summary>
    public string? Key { get; init; }

    /// <summary>The context keys this experiment declares - "user", "namespace", "project" and the like.</summary>
    public IReadOnlyList<string>? Context { get; init; }

    /// <summary>The experiment's YAML definition, when GitLab ships one for it.</summary>
    public GitLabFeatureDefinition? Definition { get; init; }

    /// <summary>Whether the experiment is currently enabled, and its Flipper rollout gate.</summary>
    public GitLabExperimentCurrentStatus? CurrentStatus { get; init; }
}