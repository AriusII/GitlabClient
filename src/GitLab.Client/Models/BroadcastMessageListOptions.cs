using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Paging options for <c>GET /broadcast_messages</c>.</summary>
[GitLabQuery]
public sealed record BroadcastMessageListOptions
{
    /// <summary>
    ///     The first page to fetch. Listing streams every following page on its own, so this skips the pages
    ///     before it rather than pinning the answer to a single page.
    /// </summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}