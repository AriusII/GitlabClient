namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for a discussion or note-level <c>PUT</c> resolve endpoint: marks the target discussion
///     or note resolved or unresolved.
/// </summary>
public sealed record ResolveDiscussionRequest
{
    /// <summary><see langword="true" /> resolves the discussion, <see langword="false" /> reopens it.</summary>
    public required bool Resolved { get; init; }
}