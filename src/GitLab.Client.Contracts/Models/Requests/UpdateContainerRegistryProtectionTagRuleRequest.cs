namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PATCH /projects/:id/registry/protection/tag/rules/:protection_rule_id</c>. Every member
///     is optional; anything left null is not sent and the rule keeps its current value. Set a level to
///     <see cref="GitLabContainerRegistryProtectionAccessLevelOrUnset.Unset" /> to clear that restriction
///     entirely.
/// </summary>
public sealed record UpdateContainerRegistryProtectionTagRuleRequest
{
    /// <summary>Repoint the rule at a different container tag name pattern.</summary>
    public string? TagNamePattern { get; init; }

    /// <summary>Raise, lower or clear the minimum role required to push a matching tag.</summary>
    public GitLabContainerRegistryProtectionAccessLevelOrUnset? MinimumAccessLevelForPush { get; init; }

    /// <summary>Raise, lower or clear the minimum role required to delete a matching tag.</summary>
    public GitLabContainerRegistryProtectionAccessLevelOrUnset? MinimumAccessLevelForDelete { get; init; }
}