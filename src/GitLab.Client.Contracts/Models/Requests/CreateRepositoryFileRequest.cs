namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/repository/files/:file_path</c>.</summary>
public sealed record CreateRepositoryFileRequest
{
    /// <summary>The branch that receives the new commit.</summary>
    public required string Branch { get; init; }

    /// <summary>The file contents, encoded according to <see cref="Encoding" />.</summary>
    public required string Content { get; init; }

    /// <summary>The commit message recorded for the file creation.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>How <see cref="Content" /> is encoded. GitLab accepts <c>text</c> and <c>base64</c>.</summary>
    public string? Encoding { get; init; }

    /// <summary>Overrides the email address recorded as the commit author.</summary>
    public string? AuthorEmail { get; init; }

    /// <summary>Overrides the name recorded as the commit author.</summary>
    public string? AuthorName { get; init; }

    /// <summary>Sets the executable bit on the newly created file.</summary>
    public bool? ExecuteFilemode { get; init; }

    /// <summary>Creates <see cref="Branch" /> from this branch, tag, or commit SHA when it does not exist.</summary>
    public string? StartBranch { get; init; }
}