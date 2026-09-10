namespace GitLab.Client.Models.Responses;

/// <summary>Cache configuration for a runner job.</summary>
public sealed record GitLabJobRequestCache
{
    public string? Key { get; init; }
    public string? Untracked { get; init; }
    public string? Paths { get; init; }
    public string? Policy { get; init; }
    public string? When { get; init; }
    public string? FallbackKeys { get; init; }
}