using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Identifies the LDAP group link removed by <c>DELETE /groups/:id/ldap_group_links</c>. The provider
///     is always required. Supply exactly one of <see cref="Cn" /> and <see cref="Filter" />; deleting by
///     filter requires GitLab Premium or Ultimate.
/// </summary>
[GitLabQuery]
public readonly record struct DeleteLdapGroupLinkOptions
{
    /// <summary>The LDAP provider that owns the link.</summary>
    public required string Provider { get; init; }

    /// <summary>The common name of the directory group to unlink. Mutually exclusive with <see cref="Filter" />.</summary>
    public string? Cn { get; init; }

    /// <summary>The LDAP filter of the link to remove. Mutually exclusive with <see cref="Cn" />.</summary>
    public string? Filter { get; init; }
}