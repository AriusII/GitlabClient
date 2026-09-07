namespace GitLab.Client.Models;

/// <summary>
///     The instance-wide default protection rule new projects' default branches get, nested in
///     <see cref="UpdateApplicationSettingsRequest.DefaultBranchProtectionDefaults" />.
/// </summary>
public sealed record GitLabBranchProtectionDefaults
{
    /// <summary>Access levels allowed to push directly to the default branch.</summary>
    public IReadOnlyList<GitLabBranchProtectionAccessRequirement>? AllowedToPush { get; init; }

    /// <summary>Allows force push for all users with push access.</summary>
    public bool? AllowForcePush { get; init; }

    /// <summary>Access levels allowed to merge into the default branch.</summary>
    public IReadOnlyList<GitLabBranchProtectionAccessRequirement>? AllowedToMerge { get; init; }

    /// <summary>Requires approval from code owners.</summary>
    public bool? CodeOwnerApprovalRequired { get; init; }

    /// <summary>Allows developers to make the branch's initial push.</summary>
    public bool? DeveloperCanInitialPush { get; init; }
}