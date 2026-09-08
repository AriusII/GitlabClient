using GitLab.Client.Domain;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Optional filters for <c>GET /groups</c>.</summary>
[GitLabQuery]
public readonly record struct GroupListOptions
{
    public string? Search { get; init; }

    public GitLabVisibility? Visibility { get; init; }

    /// <summary>Group ids to exclude from the result.</summary>
    public IReadOnlyList<long>? SkipGroups { get; init; }

    public int? PerPage { get; init; }
}