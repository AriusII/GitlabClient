namespace GitLab.Client.Models;

/// <summary>
///     An emoji reaction ("award emoji") on an awardable - an issue, a merge request, or a note on
///     either - as returned by the GitLab Award emoji API.
/// </summary>
public sealed record GitLabAwardEmoji
{
    public required long Id { get; init; }

    /// <summary>The emoji's name without surrounding colons - <c>thumbsup</c>, not <c>:thumbsup:</c>.</summary>
    public required string Name { get; init; }

    /// <summary>The user who added the reaction; only that user or an administrator may remove it.</summary>
    public GitLabUser? User { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The id of the object reacted to - an issue/merge request id, or a note id.</summary>
    public long? AwardableId { get; init; }

    /// <summary>GitLab's own name for the reacted-to object: <c>Issue</c>, <c>MergeRequest</c>, <c>Note</c>, ...</summary>
    public string? AwardableType { get; init; }

    /// <summary>Populated for custom emoji only; standard emoji report no URL.</summary>
    public Uri? Url { get; init; }
}