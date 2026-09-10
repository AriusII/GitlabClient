namespace GitLab.Client.Models;

/// <summary>A custom-attribute key/value entry embedded in a project or its owner.</summary>
public sealed record GitLabProjectCustomAttributeEntry
{
    public string? Key { get; init; }

    public string? Value { get; init; }
}