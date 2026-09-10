using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a repository tree (<c>GET /projects/:id/repository/tree</c>).</summary>
[GitLabQuery]
public readonly record struct TreeListOptions
{
    /// <summary>Branch, tag or commit SHA to read the tree from. Defaults to the project's default branch.</summary>
    public string? Ref { get; init; }

    /// <summary>Subdirectory to list, relative to the repository root. Defaults to the root.</summary>
    public string? Path { get; init; }

    /// <summary>Walk into subdirectories instead of listing one level.</summary>
    public bool? Recursive { get; init; }

    /// <summary>
    ///     Include each entry's last commit. GitLab documents this as not combinable with
    ///     <see cref="Recursive" />; the combination is rejected server-side rather than here.
    /// </summary>
    public bool? WithLastCommit { get; init; }

    public int? PerPage { get; init; }
}