namespace GitLab.Client.Models;

/// <summary>
///     The <c>push_data</c> attached to a push event.
///     <para>
///         Every member is nullable on purpose. When a push exceeds GitLab's push event activities limit
///         the instance emits one bulk event instead of per-commit events: <see cref="CommitCount" /> is
///         then 0, <see cref="RefCount" /> carries the number of refs pushed, and the individual commit
///         members are absent. That is a documented shape difference, not an error.
///     </para>
/// </summary>
public sealed record GitLabPushEventPayload
{
    /// <summary>Number of commits in the push; 0 on a bulk push event.</summary>
    public int? CommitCount { get; init; }

    /// <summary>What happened to the ref: "created", "pushed" or "removed".</summary>
    public string? Action { get; init; }

    /// <summary>"branch" or "tag".</summary>
    public string? RefType { get; init; }

    public string? CommitFrom { get; init; }

    public string? CommitTo { get; init; }

    /// <summary>The branch or tag name that was pushed to.</summary>
    public string? Ref { get; init; }

    public string? CommitTitle { get; init; }

    /// <summary>Number of refs in a bulk push event; absent on an ordinary push.</summary>
    public int? RefCount { get; init; }
}