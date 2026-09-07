namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/repository/files/:file_path</c>.</summary>
public sealed record CreateRepositoryFileRequest
{
    public required string Branch { get; init; }

    public required string Content { get; init; }

    public required string CommitMessage { get; init; }

    public string? Encoding { get; init; }
}