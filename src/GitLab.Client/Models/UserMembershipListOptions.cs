using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for a user's memberships (<c>GET /users/:id/memberships</c>). Administrators only.</summary>
[GitLabQuery]
public sealed record UserMembershipListOptions
{
    /// <summary>Return only project or only group memberships. Both are returned when unset.</summary>
    public GitLabUserMembershipType? Type { get; init; }

    public int? PerPage { get; init; }
}