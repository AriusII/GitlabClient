using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing personal access tokens (<c>GET /personal_access_tokens</c>). Administrators
///     see every token on the instance and everyone else sees only their own, so the result set can be
///     very large - which is why the listing streams rather than materialising a list.
/// </summary>
[GitLabQuery]
public sealed record PersonalAccessTokenListOptions
{
    /// <summary>Return only tokens belonging to this user. Administrators only.</summary>
    public long? UserId { get; init; }

    /// <summary>Return only tokens whose revoked state matches this value.</summary>
    public bool? Revoked { get; init; }

    /// <summary><c>active</c> or <c>inactive</c>.</summary>
    public string? State { get; init; }

    /// <summary>Filters tokens by name.</summary>
    public string? Search { get; init; }

    public string? Sort { get; init; }

    /// <summary>Return only tokens created before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>Return only tokens created after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Return only tokens last used before this instant.</summary>
    public DateTimeOffset? LastUsedBefore { get; init; }

    /// <summary>Return only tokens last used after this instant.</summary>
    public DateTimeOffset? LastUsedAfter { get; init; }

    /// <summary>Return only tokens expiring before this date. GitLab types this one as a plain date.</summary>
    public DateOnly? ExpiresBefore { get; init; }

    /// <summary>Return only tokens expiring after this date. GitLab types this one as a plain date.</summary>
    public DateOnly? ExpiresAfter { get; init; }

    public int? PerPage { get; init; }
}