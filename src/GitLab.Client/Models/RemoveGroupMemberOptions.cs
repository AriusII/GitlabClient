using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     The two switches <c>DELETE /groups/:id/members/:user_id</c> accepts, both of which change how much
///     GitLab tears down along with the membership. Passing no options applies GitLab's defaults: the
///     membership is also removed from every subgroup and project, and the user keeps their assignments.
/// </summary>
[GitLabQuery]
public readonly record struct RemoveGroupMemberOptions
{
    /// <summary>
    ///     When true, remove the membership from this group only and leave the memberships the user holds
    ///     on its subgroups and projects in place. GitLab defaults to false - a cascading removal.
    /// </summary>
    public bool? SkipSubresources { get; init; }

    /// <summary>
    ///     When true, also unassign the user from every issue and merge request in the group. GitLab
    ///     defaults to false.
    /// </summary>
    public bool? UnassignIssuables { get; init; }
}