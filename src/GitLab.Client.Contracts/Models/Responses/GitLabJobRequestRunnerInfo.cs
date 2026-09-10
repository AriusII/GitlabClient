namespace GitLab.Client.Models.Responses;

/// <summary>Runner identity and timeout data included in a job-request response.</summary>
public sealed record GitLabJobRequestRunnerInfo
{
    public string? Uuid { get; init; }
    public string? Timeout { get; init; }
    public Uri? RunnerSessionUrl { get; init; }
}