using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for listing a project's software licence policies
///     (<c>GET /projects/:id/managed_licenses</c>). GitLab declares no filters here beyond pagination.
/// </summary>
[GitLabQuery]
public sealed record ManagedLicenseListOptions
{
    public int? PerPage { get; init; }
}