namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PATCH /projects/:id/protected_branches/:name</c> and
///     <c>PATCH /groups/:id/protected_branches/:name</c>. Every property is optional: the library's
///     <c>WhenWritingNull</c> policy omits the ones left unset, so an update touches only the fields it
///     names.
/// </summary>
public sealed record UpdateProtectedBranchRequest
{
    /// <summary>Allow force push for every user who already has push access.</summary>
    public bool? AllowForcePush { get; init; }

    /// <summary>The role allowed to unprotect - 30 Developer, 40 Maintainer or 60 Admin.</summary>
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