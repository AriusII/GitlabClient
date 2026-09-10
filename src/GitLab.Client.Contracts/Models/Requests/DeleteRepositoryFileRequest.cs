namespace GitLab.Client.Models.Requests;

/// <summary>Request options for <c>DELETE /projects/:id/repository/files/:file_path</c>.</summary>
/// <remarks>
///     <see cref="LastCommitId" /> is the optimistic-concurrency guard for destructive file updates. Supply it
///     when the file was previously read from an untrusted or concurrently edited branch.
/// </remarks>
public sealed record DeleteRepositoryFileRequest
{
    /// <summary>The branch that receives the deletion commit.</summary>
    public required string Branch { get; init; }

    /// <summary>The commit message recorded for the deletion.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>Creates <see cref="Branch" /> from this branch, tag, or commit SHA when it does not exist.</summary>
    public string? StartBranch { get; init; }

    /// <summary>Overrides the email address recorded as the commit author.</summary>
    public string? AuthorEmail { get; init; }

    /// <summary>Overrides the name recorded as the commit author.</summary>
    public string? AuthorName { get; init; }

    /// <summary>
    ///     Rejects the deletion unless the file still points to this commit, preventing a lost update when another
    ///     writer changes the file first.
    /// </summary>
    public string? LastCommitId { get; init; }
}