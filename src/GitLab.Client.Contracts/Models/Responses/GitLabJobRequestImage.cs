namespace GitLab.Client.Models.Responses;

/// <summary>The primary container image requested by a CI job.</summary>
public sealed record GitLabJobRequestImage
{
    public string? Name { get; init; }
    public string? Entrypoint { get; init; }
    public GitLabJobRequestPort? Ports { get; init; }
    public string? ExecutorOpts { get; init; }
    public string? PullPolicy { get; init; }
}