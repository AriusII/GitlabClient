namespace GitLab.Client.Models;

/// <summary>The body of <c>POST /projects/:id/merge_requests</c>.</summary>
public sealed record CreateMergeRequestRequest
{
    public required string Title { get; init; }

    public required string SourceBranch { get; init; }

    public required string TargetBranch { get; init; }

    public string? Description { get; init; }
}