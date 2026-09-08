using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a group's protected branches (<c>GET /groups/:id/protected_branches</c>).
/// </summary>
[GitLabQuery]
public sealed record GroupProtectedBranchListOptions
{
    /// <summary>Return only protected branches whose name matches this text.</summary>
    public string? Search { get; init; }

    /// <summary>Items per page GitLab returns while the results are streamed. 20 by default, 100 at most.</summary>
    public int? PerPage { get; init; }
}