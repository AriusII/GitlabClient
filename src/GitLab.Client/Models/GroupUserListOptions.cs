using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters shared by the two group user listings, <c>GET /groups/:id/provisioned_users</c> and
///     <c>GET /groups/:id/saml_users</c>. Both are Premium features on a top-level group.
/// </summary>
[GitLabQuery]
public readonly record struct GroupUserListOptions
{
    /// <summary>Exact username match.</summary>
    public string? Username { get; init; }

    /// <summary>Free-text filter on name, username and email.</summary>
    public string? Search { get; init; }

    /// <summary>Keeps only users in the active state.</summary>
    public bool? Active { get; init; }

    /// <summary>Keeps only blocked users.</summary>
    public bool? Blocked { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}