namespace GitLab.Client.Models;

/// <summary>
///     A project feature flag (<c>/projects/:id/feature_flags</c>) - a named toggle whose rollout is
///     described by its <see cref="Strategies" />, and which GitLab serves to Unleash clients.
/// </summary>
public sealed record GitLabFeatureFlag
{
    /// <summary>The flag's name, which is also its route key.</summary>
    public required string Name { get; init; }

    public string? Description { get; init; }

    /// <summary>Whether the flag is enabled at all. A disabled flag is served as off to every client.</summary>
    public bool? Active { get; init; }

    /// <summary>
    ///     The flag's internal version - <c>new_version_flag</c> for every flag GitLab creates today. The
    ///     spec types it as a bare string with no enumeration, so it stays a string here: a value GitLab adds
    ///     later must not turn a healthy response into a deserialization failure.
    /// </summary>
    public string? Version { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     The flag-level environment scopes. The spec declares the array without an item type, and GitLab
    ///     answers with an empty array for every <c>new_version_flag</c> - the per-strategy
    ///     <see cref="GitLabFeatureFlagStrategy.Scopes" /> is what carries the environment scoping now.
    /// </summary>
    public IReadOnlyList<GitLabFeatureFlagScope>? Scopes { get; init; }

    /// <summary>How the flag rolls out: one entry per Unleash strategy configured on it.</summary>
    public IReadOnlyList<GitLabFeatureFlagStrategy>? Strategies { get; init; }
}