namespace GitLab.Client.Models;

/// <summary>
///     A pipeline trigger token, as returned by the GitLab CI triggers API
///     (<c>/projects/:id/triggers</c>).
/// </summary>
public sealed record GitLabTrigger
{
    public required long Id { get; init; }

    /// <summary>
    ///     Null or masked unless the authenticated user may manage this trigger - GitLab discloses the full
    ///     token only to its owner and project maintainers.
    /// </summary>
    public string? Token { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>When the token last fired a pipeline. GitLab names this field <c>last_used</c>, not <c>last_used_at</c>.</summary>
    public DateTimeOffset? LastUsed { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public GitLabUser? Owner { get; init; }
}