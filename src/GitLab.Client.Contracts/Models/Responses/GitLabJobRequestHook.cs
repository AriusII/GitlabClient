namespace GitLab.Client.Models.Responses;

/// <summary>One hook included in a runner job-request response.</summary>
public sealed record GitLabJobRequestHook
{
    public string? Name { get; init; }
    public string? Script { get; init; }
}