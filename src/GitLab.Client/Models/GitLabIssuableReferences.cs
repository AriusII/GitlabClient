using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The three ways GitLab writes a reference to an issuable (<c>APIEntitiesIssuableReferences</c>),
///     embedded as <c>references</c> in an issue or a merge request. Which one to show depends on where the
///     reference is rendered.
/// </summary>
public sealed record GitLabIssuableReferences
{
    /// <summary>
    ///     Inside the owning project: <c>#42</c>. GitLab calls the field <c>short</c>; the property cannot
    ///     (CA1720 - an identifier that is a type name), hence the explicit wire name.
    /// </summary>
    [JsonPropertyName("short")]
    public string? ShortReference { get; init; }

    /// <summary>Relative to the project being viewed: <c>gitlab#42</c>.</summary>
    public string? Relative { get; init; }

    /// <summary>Fully qualified: <c>gitlab-org/gitlab#42</c>.</summary>
    public string? Full { get; init; }
}