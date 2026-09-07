using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a group's direct members (<c>GET /groups/:id/members</c>). Members inherited
///     from an ancestor group are never returned by that route regardless of these filters - use the
///     <c>/members/all</c> route and <see cref="AllMemberListOptions" /> for those.
/// </summary>
[GitLabQuery]
public sealed record GroupMemberListOptions
{
    /// <summary>
    ///     A search term matched against member name, username and email. GitLab names this parameter
    ///     literally <c>query</c>; it is a search string, not a filter expression.
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    ///     Return only these user IDs. Sent in the repeated form (<c>user_ids[]=1&amp;user_ids[]=2</c>),
    ///     which is what Grape's array coercion expects here - the comma-joined form is not split.
    /// </summary>
    [QueryParameter("user_ids", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<long>? UserIds { get; init; }

    /// <summary>User IDs to leave out of the result, in the same repeated form as <see cref="UserIds" />.</summary>
    [QueryParameter("skip_users", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<long>? SkipUsers { get; init; }

    /// <summary>
    ///     Include seat information (<c>is_using_seat</c>) on each member. Costs GitLab an extra lookup per
    ///     member, so it is off by default.
    /// </summary>
    public bool? ShowSeatInfo { get; init; }

    /// <summary>Return only members with a linked SAML identity. Group SAML instances only.</summary>
    public bool? WithSamlIdentity { get; init; }

    public int? PerPage { get; init; }
}