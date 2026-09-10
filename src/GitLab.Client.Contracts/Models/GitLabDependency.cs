namespace GitLab.Client.Models;

/// <summary>
///     One entry of a project's dependency list (<c>GET /projects/:id/dependencies</c>) - a package the
///     dependency scanning job found, with the vulnerabilities and licences GitLab knows about for it.
/// </summary>
public sealed record GitLabDependency
{
    /// <summary>The package name, as the ecosystem spells it (<c>rails</c>, <c>@scope/package</c>).</summary>
    public required string Name { get; init; }

    /// <summary>The resolved version, as the lock file records it.</summary>
    public string? Version { get; init; }

    /// <summary>
    ///     The ecosystem the package came from - <c>bundler</c>, <c>yarn</c>, <c>npm</c>, <c>pnpm</c>,
    ///     <c>bun</c>, <c>maven</c>, <c>composer</c>, <c>pip</c>, <c>conan</c>, <c>go</c>, <c>nuget</c>,
    ///     <c>sbt</c> and more. Deliberately a bare string: GitLab types it as one and adds ecosystems
    ///     regularly, and a value added after this package shipped must not turn a healthy response into a
    ///     JSON exception.
    /// </summary>
    public string? PackageManager { get; init; }

    /// <summary>The repository-relative path of the manifest or lock file the entry was read from.</summary>
    public string? DependencyFilePath { get; init; }

    /// <summary>
    ///     The vulnerabilities affecting this package. GitLab omits the member entirely unless the caller
    ///     may read the project's vulnerabilities.
    /// </summary>
    public IReadOnlyList<GitLabDependencyVulnerability>? Vulnerabilities { get; init; }

    /// <summary>The licences the package is distributed under.</summary>
    public IReadOnlyList<GitLabDependencyLicense>? Licenses { get; init; }

    /// <summary>
    ///     Whether the package carries a GitLab Advisory Malware identifier. Omitted when malware detection
    ///     is not enabled for the project.
    /// </summary>
    public bool? Malware { get; init; }
}