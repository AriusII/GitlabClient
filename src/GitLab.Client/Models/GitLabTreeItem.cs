namespace GitLab.Client.Models;

/// <summary>
///     One entry - a file, directory or submodule - in a repository tree listing
///     (<c>GET /projects/:id/repository/tree</c>). <see cref="Type" /> is GitLab's own wire value
///     ("tree", "blob" or "commit") and stays a plain string per this library's convention of not
///     modelling GitLab's open-ended status vocabularies as enums.
/// </summary>
public sealed record GitLabTreeItem
{
    /// <summary>The object's Git SHA - a blob SHA for a file, a tree SHA for a directory.</summary>
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>"tree" for a directory, "blob" for a file, "commit" for a submodule.</summary>
    public required string Type { get; init; }

    /// <summary>Full path from the repository root, for example <c>doc/api/repositories.md</c>.</summary>
    public required string Path { get; init; }

    /// <summary>The Git file mode, for example <c>100644</c>.</summary>
    public string? Mode { get; init; }

    /// <summary>Populated only when the request asked for <c>with_last_commit</c>.</summary>
    public GitLabCommit? LastCommit { get; init; }
}