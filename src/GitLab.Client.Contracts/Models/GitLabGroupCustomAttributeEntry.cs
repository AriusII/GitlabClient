namespace GitLab.Client.Models;

/// <summary>The optional custom-attribute object embedded in a group response.</summary>
public sealed record GitLabGroupCustomAttributeEntry
{
    public string? Key { get; init; }

    public string? Value { get; init; }
}