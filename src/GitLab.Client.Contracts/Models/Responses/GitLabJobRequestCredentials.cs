namespace GitLab.Client.Models.Responses;

/// <summary>External credentials supplied to a runner for a job.</summary>
public sealed record GitLabJobRequestCredentials
{
    public string? Type { get; init; }
    public Uri? Url { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}