using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing every release in a group (<c>GET /groups/:id/releases</c>).</summary>
[GitLabQuery]
public sealed record GroupReleaseListOptions
{
    /// <summary>"asc" or "desc". GitLab defaults to "desc".</summary>
    public string? Sort { get; init; }

    /// <summary>Return only a limited set of fields for each release.</summary>
    public bool? Simple { get; init; }

    public int? PerPage { get; init; }
}