using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a group's packages across every format and every project inside it
///     (<c>GET /groups/:id/packages</c>). Separate from <see cref="PackageListOptions" /> because this
///     route additionally accepts <see cref="ExcludeSubgroups" />, and its <see cref="OrderBy" />
///     vocabulary (<see cref="GitLabGroupPackageOrderBy" />) adds a <c>project_path</c> value the project
///     route does not recognize.
/// </summary>
[GitLabQuery]
public readonly record struct GroupPackageListOptions
{
    /// <summary>Excludes packages belonging to the group's subgroups.</summary>
    public bool? ExcludeSubgroups { get; init; }

    public int? Page { get; init; }

    public int? PerPage { get; init; }

    public GitLabGroupPackageOrderBy? OrderBy { get; init; }

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