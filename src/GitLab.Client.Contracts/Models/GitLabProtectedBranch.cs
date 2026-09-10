namespace GitLab.Client.Models;

/// <summary>A protected branch (or wildcard pattern) on a project, as returned by the GitLab Protected Branches API.</summary>
public sealed record GitLabProtectedBranch
{
    /// <summary>
    ///     Nullable rather than required: GitLab omits it from some older project-scoped payloads, and only the
    ///     group-scoped entity documents it.
    /// </summary>
    public long? Id { get; init; }

    public required string Name { get; init; }

    public bool? AllowForcePush { get; init; }

    public IReadOnlyList<GitLabAccessLevel>? PushAccessLevels { get; init; }

    public IReadOnlyList<GitLabAccessLevel>? MergeAccessLevels { get; init; }

    public IReadOnlyList<GitLabAccessLevel>? UnprotectAccessLevels { get; init; }

    /// <summary>Whether pushes are blocked when the branch matches an entry in CODEOWNERS.</summary>
    public bool? CodeOwnerApprovalRequired { get; init; }

    /// <summary>
    ///     Whether the protection comes from a group-level rule rather than the project itself. Project-scoped
    ///     responses only - the group entity does not carry it.
    /// </summary>
    public bool? Inherited { get; init; }
}