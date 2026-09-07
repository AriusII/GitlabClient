using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for the namespaces a project can be transferred into
///     (<c>GET /projects/:id/transfer_locations</c>).
/// </summary>
[GitLabQuery]
public sealed record ProjectTransferLocationListOptions
{
    /// <summary>Return only namespaces matching this search term.</summary>
    public string? Search { get; init; }

    public int? PerPage { get; init; }
}