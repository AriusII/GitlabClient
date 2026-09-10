namespace GitLab.Client.Models;

/// <summary>
///     A package protection rule (<c>/projects/:id/packages/protection/rules</c>) - which packages, by
///     name pattern and package format, only members at or above a given role may push or delete.
/// </summary>
public sealed record GitLabPackageProtectionRule
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The package name pattern the rule protects, e.g. <c>@my-scope/my-package-*</c>.</summary>
    public string? PackageNamePattern { get; init; }

    /// <summary>
    ///     The package format the rule applies to (<c>npm</c>, <c>maven</c>, ...). A string here because
    ///     GitLab types the response field as free text; the request side is the typed
    ///     <see cref="GitLabPackageProtectionRuleType" />.
    /// </summary>
    public string? PackageType { get; init; }

    /// <summary>
    ///     The minimum role required to delete a matching package - <c>owner</c> or <c>admin</c>, or
    ///     <c>null</c> for GitLab's default. A string for the same reason as <see cref="PackageType" />.
    /// </summary>
    public string? MinimumAccessLevelForDelete { get; init; }

    /// <summary>
    ///     The minimum role required to push a matching package - <c>maintainer</c>, <c>owner</c> or
    ///     <c>admin</c>, or <c>null</c> for GitLab's default. A string for the same reason as
    ///     <see cref="PackageType" />.
    /// </summary>
    public string? MinimumAccessLevelForPush { get; init; }
}