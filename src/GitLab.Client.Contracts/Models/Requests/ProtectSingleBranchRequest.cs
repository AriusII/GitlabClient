namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/repository/branches/:branch/protect</c>, the branch-level
///     protection toggle. It is a coarser tool than the Protected branches API
///     (<c>POST /projects/:id/protected_branches</c>, see <see cref="ProtectBranchRequest" />), which is
///     what to reach for when access has to be expressed per role or per user.
/// </summary>
public sealed record ProtectSingleBranchRequest
{
    /// <summary>Lets Developers push directly to the branch. GitLab defaults this to false.</summary>
    public bool? DevelopersCanPush { get; init; }

    /// <summary>Lets Developers merge into the branch. GitLab defaults this to false.</summary>
    public bool? DevelopersCanMerge { get; init; }
}