using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for downloading a repository archive (<c>GET /projects/:id/repository/archive</c>).
///     GitLab.com rate-limits this endpoint to five requests a minute.
/// </summary>
[GitLabQuery]
public sealed record RepositoryArchiveOptions
{
    /// <summary>The commit SHA, branch or tag to archive. GitLab uses the default branch when unset.</summary>
    public string? Sha { get; init; }

    /// <summary>Resolves <see cref="Sha" /> as a branch or a tag when the name is ambiguous.</summary>
    public GitLabArchiveRefType? RefType { get; init; }

    /// <summary>
    ///     The archive format: <c>tar.gz</c> (GitLab's default), <c>tar.bz2</c>, <c>tbz</c>, <c>tbz2</c>,
    ///     <c>tb2</c>, <c>bz2</c>, <c>tar</c> or <c>zip</c>.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>Archives only this subdirectory of the repository.</summary>
    public string? Path { get; init; }

    /// <summary>Resolves LFS pointers to their real contents instead of archiving the pointer files.</summary>
    public bool? IncludeLfsBlobs { get; init; }

    /// <summary>Paths to leave out of the archive.</summary>
    public IReadOnlyList<string>? ExcludePaths { get; init; }
}