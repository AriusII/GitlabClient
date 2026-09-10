namespace GitLab.Client.Models;

/// <summary>An LDAP group link embedded in a detailed group response.</summary>
public sealed record GitLabGroupLdapLink
{
    public string? Cn { get; init; }

    public int? GroupAccess { get; init; }

    public string? Provider { get; init; }

    public string? Filter { get; init; }

    public long? MemberRoleId { get; init; }
}