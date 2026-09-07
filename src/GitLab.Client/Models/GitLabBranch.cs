namespace GitLab.Client.Models;

/// <summary>A branch in a project's repository, as returned by the GitLab Branches API.</summary>
public sealed record GitLabBranch
{
    public required string Name { get; init; }

    public GitLabCommit? Commit { get; init; }

    public bool? Merged { get; init; }

    public bool? Protected { get; init; }

    /// <summary>Whether the requesting user may push to this branch.</summary>
    public bool? CanPush { get; init; }

    public bool? Default { get; init; }

    public required Uri WebUrl { get; init; }

    /// <summary>
    ///     Whether Developers may push to this branch. Filled by the branch-level protection endpoints
    ///     (<c>PUT /projects/:id/repository/branches/:branch/protect</c> and its <c>unprotect</c> sibling);
    ///     the plain branch listings leave it <see langword="null" />.
    /// </summary>
    public bool? DevelopersCanPush { get; init; }

    /// <summary>Whether Developers may merge into this branch. See <see cref="DevelopersCanPush" />.</summary>
    public bool? DevelopersCanMerge { get; init; }
}