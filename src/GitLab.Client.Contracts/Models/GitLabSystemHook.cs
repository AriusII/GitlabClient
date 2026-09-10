namespace GitLab.Client.Models;

/// <summary>
///     An instance-level system hook, as returned by the GitLab System Hooks API (<c>/hooks</c>).
///     <para>
///         System hooks are an administrator surface: every endpoint under <c>/hooks</c> requires instance
///         admin rights and answers <c>403</c> otherwise. Their trigger set is deliberately narrow compared
///         with project and group hooks - pushes, tag pushes, merge requests and repository updates, plus
///         the instance lifecycle events (project, group, user and key creation and deletion) that GitLab
///         sends unconditionally and does not expose as flags.
///     </para>
/// </summary>
public sealed record GitLabSystemHook
{
    public required long Id { get; init; }

    /// <summary>The endpoint GitLab posts to, with any URL variables left as <c>{name}</c> placeholders.</summary>
    public required Uri Url { get; init; }

    /// <summary>Optional display name for the hook.</summary>
    public string? Name { get; init; }

    /// <summary>Optional free-text description of what the hook is for.</summary>
    public string? Description { get; init; }

    public long? OrganizationId { get; init; }

    public bool? PushEvents { get; init; }

    /// <summary>Restricts <see cref="PushEvents" /> to branches matching this filter.</summary>
    public string? PushEventsBranchFilter { get; init; }

    /// <summary>
    ///     How <see cref="PushEventsBranchFilter" /> is read - <c>wildcard</c>, <c>regex</c> or
    ///     <c>all_branches</c>. Left as a string because the response schema does not close the vocabulary;
    ///     use <see cref="GitLabHookBranchFilterStrategy" /> when writing.
    /// </summary>
    public string? BranchFilterStrategy { get; init; }

    public bool? TagPushEvents { get; init; }

    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Fires when a repository's refs change outside a normal push (imports, mirrors, force updates).</summary>
    public bool? RepositoryUpdateEvents { get; init; }

    public bool? EnableSslVerification { get; init; }

    /// <summary>Whether a secret token is configured. The token itself is never returned.</summary>
    public bool? TokenPresent { get; init; }

    /// <summary>Whether an HMAC signing token is configured. The token itself is never returned.</summary>
    public bool? SigningTokenPresent { get; init; }

    /// <summary>Custom request-payload template, when the hook does not send the default GitLab body.</summary>
    public string? CustomWebhookTemplate { get; init; }

    /// <summary>URL variable names. Values are write-only and always come back null.</summary>
    public IReadOnlyList<GitLabHookUrlVariable>? UrlVariables { get; init; }

    /// <summary>Custom header names. Values are write-only and always come back null.</summary>
    public IReadOnlyList<GitLabHookCustomHeader>? CustomHeaders { get; init; }

    /// <summary>
    ///     Whether GitLab is still delivering to this hook - <c>executable</c>, <c>disabled</c> or
    ///     <c>temporarily_disabled</c> after repeated failures.
    /// </summary>
    public string? AlertStatus { get; init; }

    /// <summary>When a temporarily disabled hook resumes delivery.</summary>
    public DateTimeOffset? DisabledUntil { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}