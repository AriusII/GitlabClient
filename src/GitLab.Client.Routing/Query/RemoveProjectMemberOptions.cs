using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Optional cascading behaviour for <c>DELETE /projects/:id/members/:user_id</c>. Omitting both
///     values preserves GitLab's defaults.
/// </summary>
[GitLabQuery]
public readonly record struct RemoveProjectMemberOptions
{
    /// <summary>
    ///     When true, removes the direct project membership without removing memberships the user has on
    ///     descendant resources.
    /// </summary>
    public bool? SkipSubresources { get; init; }

    /// <summary>When true, also unassigns the user from the project's issues and merge requests.</summary>
    public bool? UnassignIssuables { get; init; }
}