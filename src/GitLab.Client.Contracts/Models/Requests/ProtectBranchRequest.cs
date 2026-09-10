namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/protected_branches</c> and
///     <c>POST /groups/:id/protected_branches</c>. Both routes declare the same parameters.
/// </summary>
public sealed record ProtectBranchRequest
{
    /// <summary>The branch name, or a wildcard pattern such as <c>release/*</c>.</summary>
    public required string Name { get; init; }

    public bool? AllowForcePush { get; init; }

    /// <summary>The role allowed to push - 30 Developer, 40 Maintainer, 60 Admin or 0 no one. Defaults to 40.</summary>
    public int? PushAccessLevel { get; init; }

    /// <summary>The role allowed to merge - 30 Developer, 40 Maintainer, 60 Admin or 0 no one. Defaults to 40.</summary>
    public int? MergeAccessLevel { get; init; }

    /// <summary>The role allowed to unprotect - 30 Developer, 40 Maintainer or 60 Admin. Defaults to 40.</summary>
    public int? UnprotectAccessLevel { get; init; }

    /// <summary>Users, groups, deploy keys or access levels allowed to push.</summary>
    public IReadOnlyList<ProtectedBranchAccessRequest>? AllowedToPush { get; init; }

    /// <summary>Users, groups or access levels allowed to merge.</summary>
    public IReadOnlyList<ProtectedBranchAccessRequest>? AllowedToMerge { get; init; }

    /// <summary>Users, groups or access levels allowed to unprotect.</summary>
    public IReadOnlyList<ProtectedBranchAccessRequest>? AllowedToUnprotect { get; init; }

    /// <summary>Prevent pushes to this branch when it matches an entry in CODEOWNERS.</summary>
    public bool? CodeOwnerApprovalRequired { get; init; }
}