using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's dependencies (<c>GET /projects/:id/dependencies</c>).</summary>
[GitLabQuery]
public sealed record DependencyListOptions
{
    /// <summary>
    ///     Return only dependencies belonging to these package managers - <c>bundler</c>, <c>yarn</c>,
    ///     <c>npm</c>, <c>pnpm</c>, <c>bun</c>, <c>maven</c>, <c>composer</c>, <c>pip</c>, <c>conan</c>,
    ///     <c>go</c>, <c>nuget</c>, <c>sbt</c> and the rest. GitLab declares the parameter as an array and
    ///     expects the repeated form (<c>package_manager[]=npm&amp;package_manager[]=maven</c>).
    /// </summary>
    [QueryParameter("package_manager", MultiValue = QueryMultiValueStyle.Repeated)]
    public IReadOnlyList<string>? PackageManager { get; init; }

    public int? PerPage { get; init; }
}