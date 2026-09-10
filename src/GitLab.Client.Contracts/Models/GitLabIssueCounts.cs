namespace GitLab.Client.Models;

/// <summary>How many issues matched an <c>issues_statistics</c> query, broken down by state.</summary>
public sealed record GitLabIssueCounts
{
    public int? All { get; init; }

    public int? Closed { get; init; }

    public int? Opened { get; init; }
}