using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the inherited-member routes (<c>GET /projects/:id/members/all</c> and
///     <c>GET /groups/:id/members/all</c>), which add members coming from ancestor groups and invited
///     groups to the direct ones.
///     <para>
///         Deliberately not the same type as <see cref="GroupMemberListOptions" />: these routes do not
///         honour <c>skip_users</c> or <c>with_saml_identity</c>, and GitLab runs Grape, which answers an
///         unknown query parameter with a 200 and silently ignores it rather than failing.
///     </para>
/// </summary>
[GitLabQuery]
public sealed record AllMemberListOptions
{
    /// <summary>A search term matched against member name, username and email.</summary>
    public string? Query { get; init; }

    /// <summary>
    ///     Return only these user IDs. Sent in the repeated form (<c>user_ids[]=1&amp;user_ids[]=2</c>),
    ///     which is what Grape's array coercion expects here.
    /// </summary>
    [QueryParameter("user_ids", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<long>? UserIds { get; init; }

    /// <summary>Include seat information (<c>is_using_seat</c>) on each member.</summary>
    public bool? ShowSeatInfo { get; init; }

    /// <summary>Return only members in this state - awaiting approval, or active.</summary>
    public GitLabMembershipState? State { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Which page of results to return (1-based).</summary>
    public int? Page { get; init; }
}