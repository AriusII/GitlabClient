namespace GitLab.Client.Models.Responses;

/// <summary>One execution step included in a runner job-request response.</summary>
public sealed record GitLabJobRequestStep
{
    public string? Name { get; init; }
    public string? Script { get; init; }
    public string? Timeout { get; init; }
    public string? When { get; init; }
    public string? AllowFailure { get; init; }
}