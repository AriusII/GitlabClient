using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing Debian distributions (<c>GET /projects/:id/debian_distributions</c>,
///     <c>GET /groups/:id/-/debian_distributions</c>).
/// </summary>
/// <remarks>
///     <c>page</c> is deliberately absent: paging is the transport's job, and
///     <c>IGitLabApiConnection.GetPagedAsync</c> follows the <c>Link: rel="next"</c> header rather than
///     counting pages.
/// </remarks>
[GitLabQuery]
public readonly record struct DebianDistributionListOptions
{
    public int? PerPage { get; init; }

    public string? Codename { get; init; }

    public string? Suite { get; init; }

    public string? Origin { get; init; }

    public string? Label { get; init; }

    public string? Version { get; init; }

    public string? Description { get; init; }

    public int? ValidTimeDurationSeconds { get; init; }

    public IReadOnlyList<string>? Components { get; init; }

    public IReadOnlyList<string>? Architectures { get; init; }
}