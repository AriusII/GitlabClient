namespace GitLab.Client.Models;

/// <summary>Issue counts for a group, after the request's filters have been applied.</summary>
public sealed record GitLabGroupIssueCounts
{
    public int? All { get; init; }

    public int? Opened { get; init; }

    public int? Closed { get; init; }
}