namespace GitLab.Client.Models;

/// <summary>
///     One instance-level Flipper feature (<c>/features</c>) - the flags GitLab itself is developed
///     behind, not a project's own <see cref="GitLabFeatureFlag" />. Reading and writing them requires
///     administrator access.
/// </summary>
public sealed record GitLabFeature
{
    public required string Name { get; init; }

    /// <summary>
    ///     The feature's overall state - <c>on</c>, <c>off</c> or <c>conditional</c>. The spec types it as a
    ///     bare string with no enumeration, so it stays a string here.
    /// </summary>
    public string? State { get; init; }

    /// <summary>The individual Flipper gates that make up <see cref="State" />.</summary>
    public IReadOnlyList<GitLabFeatureGate>? Gates { get; init; }

    /// <summary>The feature's YAML definition, when the instance ships one for it.</summary>
    public GitLabFeatureDefinition? Definition { get; init; }
}