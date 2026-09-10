namespace GitLab.Client.Models;

/// <summary>A custom-attribute projection embedded by a basic project or user response.</summary>
public sealed record GitLabBasicCustomAttributeEntry
{
    public string? Key { get; init; }

    public string? Value { get; init; }
}