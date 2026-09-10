using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a group's enterprise users (<c>GET /groups/:id/enterprise_users</c>).
///     Administrators and group owners only.
/// </summary>
[GitLabQuery]
public readonly record struct EnterpriseUserListOptions
{
    /// <summary>Return the single enterprise user with exactly this username.</summary>
    public string? Username { get; init; }

    /// <summary>Free-text search across name, username and public email.</summary>
    public string? Search { get; init; }

    /// <summary>Return only accounts in the <c>active</c> state.</summary>
    public bool? Active { get; init; }

    /// <summary>Return only blocked accounts.</summary>
    public bool? Blocked { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>
    ///     Filter by two-factor authentication status. Left as free text because the spec declares no
    ///     enumeration for it.
    /// </summary>
    public string? TwoFactor { get; init; }

    public int? PerPage { get; init; }
}