namespace GitLab.Client.Models;

/// <summary>
///     The NuGet Content Service index response, returned by
///     <c>GET /projects/:id/packages/nuget/download/:package_name/index</c> - every published version of
///     one package, as plain version strings.
/// </summary>
public sealed record GitLabNugetPackagesVersions
{
    public IReadOnlyList<string>? Versions { get; init; }
}