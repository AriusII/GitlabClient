namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /projects/:id/repository/files/:file_path</c>.</summary>
public sealed record UpdateRepositoryFileRequest
{
    /// <summary>The branch that receives the commit.</summary>
    public required string Branch { get; init; }

    /// <summary>The replacement file contents, encoded according to <see cref="Encoding" />.</summary>
    public required string Content { get; init; }

    /// <summary>The commit message recorded for the update.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>How <see cref="Content" /> is encoded. GitLab accepts <c>text</c> and <c>base64</c>.</summary>
    public string? Encoding { get; init; }

    /// <summary>Overrides the email address recorded as the commit author.</summary>
    public string? AuthorEmail { get; init; }

    /// <summary>Overrides the name recorded as the commit author.</summary>
    public string? AuthorName { get; init; }

    /// <summary>Sets or clears the executable bit on the file.</summary>
    public bool? ExecuteFilemode { get; init; }

    /// <summary>
    ///     Rejects the update unless the file still points to this commit, preventing a lost update when another
    ///     writer changes the file first.
    /// </summary>
    public string? LastCommitId { get; init; }

    /// <summary>Creates <see cref="Branch" /> from this branch, tag, or commit SHA when it does not exist.</summary>
    public string? StartBranch { get; init; }
}