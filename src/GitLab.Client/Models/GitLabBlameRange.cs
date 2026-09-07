namespace GitLab.Client.Models;

/// <summary>
///     One run of consecutive lines attributed to a single commit
///     (<c>GET /projects/:id/repository/files/:file_path/blame</c>). The ranges come back in file order,
///     so concatenating every <see cref="Lines" /> reproduces the file.
/// </summary>
public sealed record GitLabBlameRange
{
    /// <summary>The commit that last modified these lines.</summary>
    public GitLabBlameCommit? Commit { get; init; }

    /// <summary>The lines themselves, without trailing newlines.</summary>
    public IReadOnlyList<string>? Lines { get; init; }
}