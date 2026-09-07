using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /groups/:id/transfer_locations</c> - the groups this one may be moved under.
/// </summary>
[GitLabQuery]
public sealed record GroupTransferLocationListOptions
{
    /// <summary>Free-text filter on the name and path.</summary>
    public string? Search { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}