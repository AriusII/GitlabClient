using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /groups/:id/billable_members</c>.
/// </summary>
[GitLabQuery]
public readonly record struct GroupBillableMemberListOptions
{
    /// <summary>Free-text filter on the member's name.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     One of <c>access_level_asc</c>, <c>access_level_desc</c>, <c>last_joined</c>, <c>name_asc</c>,
    ///     <c>name_desc</c>, <c>oldest_joined</c>, <c>oldest_sign_in</c>, <c>recent_sign_in</c>, <c>last_activity_on_asc</c>
    ///     or <c>last_activity_on_desc</c>.
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}