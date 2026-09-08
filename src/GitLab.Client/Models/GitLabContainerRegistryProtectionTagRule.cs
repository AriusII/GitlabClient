namespace GitLab.Client.Models;

/// <summary>
///     A container registry protection tag rule (<c>/projects/:id/registry/protection/tag/rules</c>) -
///     which container image tags, by name pattern, only members at or above a given role may push or
///     delete. Introduced in GitLab 18.7.
/// </summary>
public sealed record GitLabContainerRegistryProtectionTagRule
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The container tag name pattern the rule protects, e.g. <c>v*-release</c>.</summary>
    public string? TagNamePattern { get; init; }

    /// <summary>
    ///     The minimum role required to push a matching tag. A string here because GitLab types the
    ///     response field as free text; the request side is the typed
    ///     <see cref="GitLabContainerRegistryProtectionAccessLevel" /> (create) or
    ///     <see cref="GitLabContainerRegistryProtectionAccessLevelOrUnset" /> (update).
    /// </summary>
    public string? MinimumAccessLevelForPush { get; init; }

    /// <summary>
    ///     The minimum role required to delete a matching tag. A string for the same reason as
    ///     <see cref="MinimumAccessLevelForPush" />.
    /// </summary>
    public string? MinimumAccessLevelForDelete { get; init; }
}