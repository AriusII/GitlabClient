namespace GitLab.Client.Models.Responses;

/// <summary>A container port included in a runner job-request response.</summary>
public sealed record GitLabJobRequestPort
{
    public string? Number { get; init; }
    public string? Protocol { get; init; }
    public string? Name { get; init; }
}