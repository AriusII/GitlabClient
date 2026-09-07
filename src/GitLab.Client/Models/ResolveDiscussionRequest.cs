namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/issues/:issue_iid/discussions/:discussion_id</c> and
///     <c>PUT /projects/:id/merge_requests/:merge_request_iid/discussions/:discussion_id</c>: marks the
///     whole thread resolved or unresolved.
/// </summary>
public sealed record ResolveDiscussionRequest
{
    /// <summary><see langword="true" /> resolves the discussion, <see langword="false" /> reopens it.</summary>
    public required bool Resolved { get; init; }
}