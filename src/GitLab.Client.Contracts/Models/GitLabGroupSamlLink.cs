namespace GitLab.Client.Models;

/// <summary>A SAML group link embedded in a detailed group response.</summary>
public sealed record GitLabGroupSamlLink
{
    public string? Name { get; init; }

    public int? AccessLevel { get; init; }

    public long? MemberRoleId { get; init; }

    public string? Provider { get; init; }
}