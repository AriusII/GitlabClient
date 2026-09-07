namespace GitLab.Client.Models;

/// <summary>
///     A single access-level entry (push/merge) on a <see cref="GitLabProtectedBranch" />, as returned
///     nested inside the GitLab Protected Branches API.
/// </summary>
public sealed record GitLabAccessLevel
{
    public required int AccessLevel { get; init; }

    public string? AccessLevelDescription { get; init; }
}