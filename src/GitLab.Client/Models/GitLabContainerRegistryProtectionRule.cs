namespace GitLab.Client.Models;

/// <summary>
///     A container repository protection rule (<c>/projects/:id/registry/protection/repository/rules</c>)
///     - which container image repositories, by path pattern, only members at or above a given role may
///     push or delete images to.
/// </summary>
public sealed record GitLabContainerRegistryProtectionRule
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The container repository path pattern the rule protects, e.g. <c>flight/flight-*</c>.</summary>
    public string? RepositoryPathPattern { get; init; }

    /// <summary>
    ///     The minimum role required to push a matching image. A string here because GitLab types the
    ///     response field as free text; the request side is the typed
    ///     <see cref="GitLabContainerRegistryProtectionAccessLevel" /> (create) or
    ///     <see cref="GitLabContainerRegistryProtectionAccessLevelOrUnset" /> (update).
    /// </summary>
    public string? MinimumAccessLevelForPush { get; init; }

    /// <summary>
    ///     The minimum role required to delete a matching image. A string for the same reason as
    ///     <see cref="MinimumAccessLevelForPush" />.
    /// </summary>
    public string? MinimumAccessLevelForDelete { get; init; }
}