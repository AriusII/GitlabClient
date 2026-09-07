using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a package's files
///     (<c>GET /projects/:id/packages/:package_id/package_files</c>).
/// </summary>
[GitLabQuery]
public sealed record PackageFileListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }

    /// <summary>Defaults to <see cref="GitLabPackageFileOrderBy.Id" /> on GitLab's side when left unset.</summary>
    public GitLabPackageFileOrderBy? OrderBy { get; init; }

    public GitLabPackageFileSort? Sort { get; init; }
}