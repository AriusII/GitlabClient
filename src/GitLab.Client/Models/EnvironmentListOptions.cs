using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's environments (<c>GET /projects/:id/environments</c>).</summary>
[GitLabQuery]
public sealed record EnvironmentListOptions
{
    /// <summary>Return the environment with this exact name.</summary>
    public string? Name { get; init; }

    /// <summary>Return environments whose name contains this text.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     A single state to filter on - <c>available</c>, <c>stopping</c> or <c>stopped</c>. Singular despite
    ///     the plural wire name: GitLab declares it as one string, not an array.
    /// </summary>
    public string? States { get; init; }

    public int? PerPage { get; init; }
}