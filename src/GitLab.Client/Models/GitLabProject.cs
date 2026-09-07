using GitLab.Client.Domain;

namespace GitLab.Client.Models;

public sealed record GitLabProject
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string PathWithNamespace { get; init; }

    public string? Description { get; init; }

    public required GitLabVisibility Visibility { get; init; }

    public required Uri WebUrl { get; init; }

    public string? DefaultBranch { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public bool? Archived { get; init; }

    public int? StarCount { get; init; }

    public int? ForksCount { get; init; }
}