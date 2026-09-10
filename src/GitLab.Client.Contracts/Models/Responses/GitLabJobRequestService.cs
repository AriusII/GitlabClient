namespace GitLab.Client.Models.Responses;

/// <summary>One service container requested by a CI job.</summary>
public sealed record GitLabJobRequestService
{
    public string? Name { get; init; }
    public string? Entrypoint { get; init; }
    public GitLabJobRequestPort? Ports { get; init; }
    public string? ExecutorOpts { get; init; }
    public string? PullPolicy { get; init; }
    public string? Alias { get; init; }
    public string? Command { get; init; }
    public string? Variables { get; init; }
}