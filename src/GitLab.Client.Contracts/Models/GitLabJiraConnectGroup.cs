namespace GitLab.Client.Models;

/// <summary>
///     The GitLab namespace behind a GitLab for Jira (Forge) subscription, as the Jira app renders it in
///     its configuration screen. A deliberately thin projection of a group - just enough to name and
///     picture it - not the full <see cref="GitLabGroup" />.
/// </summary>
public sealed record GitLabJiraConnectGroup
{
    /// <summary>The group's display name, without its ancestors.</summary>
    public string? Name { get; init; }

    /// <summary>The group's name qualified by its ancestors, for example <c>Parent / Child</c>.</summary>
    public string? FullName { get; init; }

    /// <summary>The group description, as plain text.</summary>
    public string? Description { get; init; }

    /// <summary>The group avatar, or <see langword="null" /> when the group has none.</summary>
    public Uri? AvatarUrl { get; init; }
}