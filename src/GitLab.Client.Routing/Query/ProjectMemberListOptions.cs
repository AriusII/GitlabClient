using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a project's direct members (<c>GET /projects/:id/members</c>). Inherited
///     members are intentionally excluded by that route; use <see cref="AllMemberListOptions" /> to
///     search the effective membership instead.
/// </summary>
[GitLabQuery]
public readonly record struct ProjectMemberListOptions
{
    /// <summary>A search term matched against the member name, username, and email address.</summary>
    public string? Query { get; init; }

    /// <summary>
    ///     Restricts the result to these user IDs. GitLab expects the repeated
    ///     <c>user_ids[]=1&amp;user_ids[]=2</c> representation.
    /// </summary>
    [QueryParameter("user_ids", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<long>? UserIds { get; init; }

    /// <summary>User IDs to omit from the result, in the same repeated representation as <see cref="UserIds" />.</summary>
    [QueryParameter("skip_users", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<long>? SkipUsers { get; init; }

    /// <summary>Includes the <c>is_using_seat</c> value for each returned member.</summary>
    public bool? ShowSeatInfo { get; init; }

    /// <summary>Restricts the result to members with a linked group SAML identity.</summary>
    public bool? WithSamlIdentity { get; init; }

    /// <summary>Which page of results to return (1-based).</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}