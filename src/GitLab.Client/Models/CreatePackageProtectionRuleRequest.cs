namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/packages/protection/rules</c>.
/// </summary>
public sealed record CreatePackageProtectionRuleRequest
{
    /// <summary>
    ///     The package name pattern to protect, e.g. <c>@my-scope/my-package-*</c>. The wildcard character
    ///     <c>*</c> is allowed.
    /// </summary>
    public required string PackageNamePattern { get; init; }

    /// <summary>The package format the rule applies to.</summary>
    public required GitLabPackageProtectionRuleType PackageType { get; init; }

    /// <summary>Omit to take GitLab's default minimum role for deleting a matching package.</summary>
    public GitLabPackageProtectionRuleDeleteAccessLevel? MinimumAccessLevelForDelete { get; init; }

    /// <summary>Omit to take GitLab's default minimum role for pushing a matching package.</summary>
    public GitLabPackageProtectionRulePushAccessLevel? MinimumAccessLevelForPush { get; init; }
}