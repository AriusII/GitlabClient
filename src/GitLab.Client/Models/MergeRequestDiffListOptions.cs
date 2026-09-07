using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Options for <c>GET /projects/:id/merge_requests/:merge_request_iid/diffs</c>, the paginated sibling
///     of the <c>changes</c> endpoint.
/// </summary>
[GitLabQuery]
public sealed record MergeRequestDiffListOptions
{
    /// <summary>
    ///     Presents the diffs in the Git unified format rather than GitLab's own, which matters for any
    ///     consumer feeding them to a patch tool.
    /// </summary>
    public bool? Unidiff { get; init; }

    public int? PerPage { get; init; }
}