namespace GitLab.Client.Models;

/// <summary>
///     The default-branch protection a group applies to the projects created inside it - GitLab's
///     replacement for the single numeric <c>default_branch_protection</c> level.
/// </summary>
public sealed record GitLabDefaultBranchProtectionDefaults
{
    /// <summary>Roles allowed to push to the default branch, as access-level entries.</summary>
    public IReadOnlyList<GitLabAccessLevel>? AllowedToPush { get; init; }

    /// <summary>Roles allowed to merge into the default branch, as access-level entries.</summary>
    public IReadOnlyList<GitLabAccessLevel>? AllowedToMerge { get; init; }

    public bool? AllowForcePush { get; init; }

    /// <summary>Whether a developer may push the very first commit into an otherwise empty repository.</summary>
    public bool? DeveloperCanInitialPush { get; init; }

    public bool? CodeOwnerApprovalRequired { get; init; }
}