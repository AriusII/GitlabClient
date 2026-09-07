namespace GitLab.Client.Models;

public sealed record CreateMergeRequestRequest
{
    public required string Title { get; init; }

    public required string SourceBranch { get; init; }

    public required string TargetBranch { get; init; }

    public string? Description { get; init; }
}