namespace GitLab.Client.Models;

/// <summary>The container-registry expiry policy returned with an expanded project response.</summary>
public sealed record GitLabProjectContainerExpirationPolicy
{
    public string? Cadence { get; init; }

    public string? Enabled { get; init; }

    public string? KeepN { get; init; }

    public string? OlderThan { get; init; }

    public string? NameRegex { get; init; }

    public string? NameRegexKeep { get; init; }

    public string? NextRunAt { get; init; }
}