namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PATCH /projects/:id/registry/protection/repository/rules/:protection_rule_id</c>. Every
///     member is optional; anything left null is not sent and the rule keeps its current value. Set a
///     level to <see cref="GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset" /> to clear that
///     restriction entirely.
/// </summary>
public sealed record UpdateContainerRegistryProtectionRuleRequest
{
    /// <summary>Repoint the rule at a different container repository path pattern.</summary>
    public string? RepositoryPathPattern { get; init; }

    /// <summary>Raise, lower or clear the minimum role required to push a matching image.</summary>
    public GitLabContainerRegistryProtectionAccessLevelOrUnset? MinimumAccessLevelForPush { get; init; }

    /// <summary>Raise, lower or clear the minimum role required to delete a matching image.</summary>
    public GitLabContainerRegistryProtectionAccessLevelOrUnset? MinimumAccessLevelForDelete { get; init; }
}