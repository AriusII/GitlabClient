namespace GitLab.Client.Models;

/// <summary>
///     A feature flag definition (<c>GET /features/definitions</c>) - the YAML file GitLab ships
///     alongside each of its own development flags, describing who owns it and how it is meant to roll
///     out.
///     <para>
///         Every member is nullable, including <see cref="Name" />: the same entity is embedded as
///         <see cref="GitLabFeature.Definition" />, and a definition file only has to declare the keys its
///         flag type requires.
///     </para>
/// </summary>
public sealed record GitLabFeatureDefinition
{
    /// <summary>The flag name the definition belongs to.</summary>
    public string? Name { get; init; }

    /// <summary>The issue describing the feature the flag guards.</summary>
    public Uri? FeatureIssueUrl { get; init; }

    /// <summary>The merge request that introduced the flag.</summary>
    public Uri? IntroducedByUrl { get; init; }

    /// <summary>The issue tracking the flag's rollout and eventual removal.</summary>
    public Uri? RolloutIssueUrl { get; init; }

    /// <summary>The GitLab milestone the flag was introduced in.</summary>
    public string? Milestone { get; init; }

    /// <summary>Whether GitLab writes an audit entry every time the flag is toggled.</summary>
    public bool? LogStateChanges { get; init; }

    /// <summary>
    ///     The flag type - <c>development</c>, <c>ops</c>, <c>experiment</c>, <c>gitlab_com_derisk</c>,
    ///     <c>worker</c>, <c>beta</c>. The spec types it as a bare string with no enumeration, so it stays a
    ///     string here.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>The GitLab team that owns the flag, in the <c>group::name</c> form.</summary>
    public string? Group { get; init; }

    /// <summary>Whether the flag is on by default when no gate has been set.</summary>
    public bool? DefaultEnabled { get; init; }

    /// <summary>The milestone the flag is meant to be fully rolled out by, as free text.</summary>
    public string? IntendedToRolloutBy { get; init; }
}