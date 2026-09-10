namespace GitLab.Client.Models;

/// <summary>
///     A reviewer of a merge request together with their review state
///     (<c>GET /projects/:id/merge_requests/:iid/reviewers</c>).
/// </summary>
public sealed record GitLabMergeRequestReviewer
{
    /// <summary>The reviewer.</summary>
    public GitLabUser? User { get; init; }

    /// <summary>
    ///     The reviewer's state - "unreviewed", "reviewed", "requested_changes", "approved", ... Left as a
    ///     string on purpose: GitLab types this as a bare string in the spec and adds values over time, and a
    ///     new one must not turn a healthy response into a <c>JsonException</c>.
    /// </summary>
    public string? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}