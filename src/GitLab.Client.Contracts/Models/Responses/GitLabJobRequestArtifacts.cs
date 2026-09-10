namespace GitLab.Client.Models.Responses;

/// <summary>Artifact upload configuration for a runner job.</summary>
public sealed record GitLabJobRequestArtifacts
{
    public string? Name { get; init; }
    public string? Untracked { get; init; }
    public string? Paths { get; init; }
    public string? Exclude { get; init; }
    public string? When { get; init; }
    public string? ExpireIn { get; init; }
    public string? ArtifactType { get; init; }
    public string? ArtifactFormat { get; init; }
}