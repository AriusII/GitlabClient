using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for listing a project's commits (<c>GET /projects/:id/repository/commits</c>).</summary>
[GitLabQuery]
public readonly record struct CommitListOptions
{
    /// <summary>Returns every commit, ignoring <see cref="RefName" /> when <see langword="true" />.</summary>
    public bool? All { get; init; }

    /// <summary>Filters commits by the commit author.</summary>
    public string? Author { get; init; }

    /// <summary>Follows only the first parent when traversing merge commits.</summary>
    public bool? FirstParent { get; init; }

    /// <summary>
    ///     Follows file renames while filtering by <see cref="Path" />. This affects a single-file path
    ///     filter only and is supported by GitLab 18.10 and later.
    /// </summary>
    public bool? Follow { get; init; }

    /// <summary>Either GitLab's <c>default</c> reverse-chronological order or Git's <c>topo</c> order.</summary>
    public string? Order { get; init; }

    /// <summary>Limits the history to commits touching this repository-relative file path.</summary>
    public string? Path { get; init; }

    public string? RefName { get; init; }

    public DateTimeOffset? Since { get; init; }

    public DateTimeOffset? Until { get; init; }

    /// <summary>Asks GitLab to parse Git trailers for every returned commit.</summary>
    public bool? Trailers { get; init; }

    /// <summary>Includes addition, deletion and total statistics for every returned commit.</summary>
    public bool? WithStats { get; init; }

    public int? PerPage { get; init; }
}