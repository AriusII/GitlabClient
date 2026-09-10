namespace GitLab.Client.Models;

/// <summary>The caller's inherited access to a project through a group.</summary>
public sealed record GitLabProjectGroupAccess
{
    public string? AccessLevel { get; init; }

    public string? NotificationLevel { get; init; }
}