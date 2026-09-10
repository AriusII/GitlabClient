using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing a group's protected branches (<c>GET /groups/:id/protected_branches</c>).
/// </summary>
[GitLabQuery]
public readonly record struct GroupProtectedBranchListOptions
{
    /// <summary>Return only protected branches whose name matches this text.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     The first offset page to stream. Subsequent pages are followed automatically from GitLab's
    ///     <c>Link</c> header.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}