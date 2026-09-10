using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing users (<c>GET /users</c>).
///     <para>
///         The first four are lookups rather than filters - GitLab answers each with the single matching
///         account. Several of the rest (<see cref="Blocked" />, <see cref="Admins" />,
///         <see cref="TwoFactor" />, <see cref="SkipLdap" />, the <c>Exclude*</c> family) are honoured
///         for administrators only and are silently ignored otherwise, which is GitLab's behaviour, not
///         this package's.
///     </para>
/// </summary>
[GitLabQuery]
public sealed record UserListOptions
{
    /// <summary>Return the single user with exactly this username.</summary>
    public string? Username { get; init; }

    /// <summary>Return the single user with this UID at <see cref="Provider" />. Administrators only.</summary>
    public string? ExternUid { get; init; }

    /// <summary>Return the single user who published exactly this email on their profile.</summary>
    public string? PublicEmail { get; init; }

    /// <summary>The external authentication provider <see cref="ExternUid" /> is looked up in.</summary>
    public string? Provider { get; init; }

    /// <summary>Free-text search across name, username and public email.</summary>
    public string? Search { get; init; }

    /// <summary>Return only accounts in the <c>active</c> state.</summary>
    public bool? Active { get; init; }

    /// <summary>Return only human accounts, excluding bots and service accounts.</summary>
    public bool? Humans { get; init; }

    /// <summary>Return only external users.</summary>
    public bool? External { get; init; }

    /// <summary>Return only blocked accounts. Administrators only.</summary>
    public bool? Blocked { get; init; }

    /// <summary>Return only instance administrators. Administrators only.</summary>
    public bool? Admins { get; init; }

    /// <summary>Return only auditor accounts. Administrators only, GitLab Premium and above.</summary>
    public bool? Auditors { get; init; }

    /// <summary>
    ///     Filter by two-factor authentication: <c>enabled</c> or <c>disabled</c>. Administrators only.
    ///     Left as free text because the spec declares no vocabulary for it.
    /// </summary>
    public string? TwoFactor { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>Return only accounts that own no projects. Administrators only.</summary>
    public bool? WithoutProjects { get; init; }

    /// <summary>Exclude project and group access-token bot accounts.</summary>
    public bool? WithoutProjectBots { get; init; }

    /// <summary>Exclude accounts in the <c>active</c> state. Administrators only.</summary>
    public bool? ExcludeActive { get; init; }

    /// <summary>Exclude external users.</summary>
    public bool? ExcludeExternal { get; init; }

    /// <summary>Exclude human accounts, leaving bots and service accounts.</summary>
    public bool? ExcludeHumans { get; init; }

    /// <summary>Exclude GitLab's own internal accounts (the support and alert bots).</summary>
    public bool? ExcludeInternal { get; init; }

    /// <summary>Exclude LDAP-synchronised accounts. Administrators only.</summary>
    public bool? SkipLdap { get; init; }

    /// <summary>Include each user's custom attributes in the response. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    public GitLabUserOrderBy? OrderBy { get; init; }

    public GitLabUserSort? Sort { get; init; }

    public int? PerPage { get; init; }
}