using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the namespaces a project can be transferred into
///     (<c>GET /projects/:id/transfer_locations</c>).
/// </summary>
[GitLabQuery]
public readonly record struct ProjectTransferLocationListOptions
{
    /// <summary>Return only namespaces matching this search term.</summary>
    public string? Search { get; init; }

    public int? PerPage { get; init; }
}