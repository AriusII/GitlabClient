using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the groups a project can be shared with (<c>GET /projects/:id/share_locations</c>).
///     The endpoint declares no pagination parameters of its own.
/// </summary>
[GitLabQuery]
public readonly record struct ProjectShareLocationListOptions
{
    /// <summary>Return only groups matching this search term.</summary>
    public string? Search { get; init; }
}