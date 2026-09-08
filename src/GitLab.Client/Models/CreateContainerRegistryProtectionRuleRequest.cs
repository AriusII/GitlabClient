namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/registry/protection/repository/rules</c>.
/// </summary>
public sealed record CreateContainerRegistryProtectionRuleRequest
{
    /// <summary>
    ///     The container repository path pattern to protect, e.g. <c>flight/flight-*</c>. The wildcard
    ///     character <c>*</c> is allowed.
    /// </summary>
    public required string RepositoryPathPattern { get; init; }

    /// <summary>Omit to leave pushing a matching image unrestricted.</summary>
    public GitLabContainerRegistryProtectionAccessLevel? MinimumAccessLevelForPush { get; init; }

    /// <summary>Omit to leave deleting a matching image unrestricted.</summary>
    public GitLabContainerRegistryProtectionAccessLevel? MinimumAccessLevelForDelete { get; init; }
}