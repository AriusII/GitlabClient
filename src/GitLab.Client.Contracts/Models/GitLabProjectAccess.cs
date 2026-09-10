namespace GitLab.Client.Models;

/// <summary>The caller's direct access to a project.</summary>
public sealed record GitLabProjectAccess
{
    public string? AccessLevel { get; init; }

    public string? NotificationLevel { get; init; }
}