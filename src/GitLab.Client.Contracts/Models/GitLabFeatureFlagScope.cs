namespace GitLab.Client.Models;

/// <summary>
///     An environment scope on a <see cref="GitLabFeatureFlagStrategy" /> - which environments the
///     strategy applies to. <c>*</c> means every environment.
/// </summary>
public sealed record GitLabFeatureFlagScope
{
    public long? Id { get; init; }

    /// <summary>The environment name or wildcard the strategy is scoped to - <c>*</c>, <c>production</c>, <c>review/*</c>.</summary>
    public string? EnvironmentScope { get; init; }
}