using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a project's packages across every format (<c>GET /projects/:id/packages</c>).
///     Separate from <see cref="GroupPackageListOptions" /> because the group route additionally accepts
///     <c>exclude_subgroups</c> and an extra <c>project_path</c> value for its own order-by vocabulary,
///     neither of which the project route declares - sharing one record would let those be sent to the
///     project route, where GitLab answers 200 and silently ignores them.
/// </summary>
[GitLabQuery]
public readonly record struct PackageListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }

    public GitLabPackageOrderBy? OrderBy { get; init; }

    public GitLabPackageSort? Sort { get; init; }

    /// <summary>Restricts results to one package format.</summary>
    public GitLabPackageType? PackageType { get; init; }

    /// <summary>Exact package name match.</summary>
    public string? PackageName { get; init; }

    /// <summary>Exact package version match.</summary>
    public string? PackageVersion { get; init; }

    /// <summary>Includes packages that have no version.</summary>
    public bool? IncludeVersionless { get; init; }

    /// <summary>
    ///     Restricts results to this publication status. GitLab returns <c>default</c>, <c>deprecated</c>
    ///     and <c>error</c> packages when this is left unset.
    /// </summary>
    public GitLabPackageStatus? Status { get; init; }
}