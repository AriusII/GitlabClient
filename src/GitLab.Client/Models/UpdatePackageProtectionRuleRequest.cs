namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PATCH /projects/:id/packages/protection/rules/:package_protection_rule_id</c>. Every
///     member is optional; anything left null is not sent and the rule keeps its current value.
/// </summary>
public sealed record UpdatePackageProtectionRuleRequest
{
    /// <summary>Repoint the rule at a different package name pattern.</summary>
    public string? PackageNamePattern { get; init; }

    /// <summary>Change the package format the rule applies to.</summary>
    public GitLabPackageProtectionRuleType? PackageType { get; init; }

    /// <summary>Raise or lower the minimum role required to delete a matching package.</summary>
    public GitLabPackageProtectionRuleDeleteAccessLevel? MinimumAccessLevelForDelete { get; init; }

    /// <summary>Raise or lower the minimum role required to push a matching package.</summary>
    public GitLabPackageProtectionRulePushAccessLevel? MinimumAccessLevelForPush { get; init; }
}