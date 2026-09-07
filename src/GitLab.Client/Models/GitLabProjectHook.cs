namespace GitLab.Client.Models;

/// <summary>
///     A project-level webhook, as returned by the GitLab Project Hooks API (<c>/projects/:id/hooks</c>).
///     <para>
///         The secrets are deliberately absent: GitLab never echoes <c>token</c> or <c>signing_token</c>
///         back, and only reports whether one is configured through <see cref="TokenPresent" /> and
///         <see cref="SigningTokenPresent" />. The same holds for the values of
///         <see cref="UrlVariables" /> and <see cref="CustomHeaders" /> - the keys come back, the values
///         do not.
///     </para>
/// </summary>
public sealed record GitLabProjectHook
{
    public required long Id { get; init; }

    /// <summary>The endpoint GitLab posts to, with any URL variables left as <c>{name}</c> placeholders.</summary>
    public required Uri Url { get; init; }

    /// <summary>Optional display name for the hook.</summary>
    public string? Name { get; init; }

    /// <summary>Optional free-text description of what the hook is for.</summary>
    public string? Description { get; init; }

    /// <summary>The project the hook belongs to.</summary>
    public long? ProjectId { get; init; }

    public long? OrganizationId { get; init; }

    public bool? PushEvents { get; init; }

    /// <summary>Restricts <see cref="PushEvents" /> to branches matching this filter.</summary>
    public string? PushEventsBranchFilter { get; init; }

    /// <summary>
    ///     How <see cref="PushEventsBranchFilter" /> is read - <c>wildcard</c>, <c>regex</c> or
    ///     <c>all_branches</c>. Left as a string because GitLab's response schema does not close the
    ///     vocabulary; use <see cref="GitLabHookBranchFilterStrategy" /> when writing.
    /// </summary>
    public string? BranchFilterStrategy { get; init; }

    public bool? IssuesEvents { get; init; }

    public bool? ConfidentialIssuesEvents { get; init; }

    public bool? MergeRequestsEvents { get; init; }

    public bool? TagPushEvents { get; init; }

    public bool? NoteEvents { get; init; }

    public bool? ConfidentialNoteEvents { get; init; }

    public bool? JobEvents { get; init; }

    public bool? PipelineEvents { get; init; }

    public bool? WikiPageEvents { get; init; }

    public bool? DeploymentEvents { get; init; }

    public bool? FeatureFlagEvents { get; init; }

    public bool? ReleasesEvents { get; init; }

    public bool? MilestoneEvents { get; init; }

    public bool? EmojiEvents { get; init; }

    public bool? RepositoryUpdateEvents { get; init; }

    public bool? VulnerabilityEvents { get; init; }

    /// <summary>Fires when a project access token is about to expire.</summary>
    public bool? ResourceAccessTokenEvents { get; init; }

    /// <summary>Fires when a deploy token is about to expire.</summary>
    public bool? ResourceDeployTokenEvents { get; init; }

    /// <summary>Whether Duo Agent Platform flows may post lifecycle events to this webhook.</summary>
    public bool? DuoFlowCallbackEnabled { get; init; }

    public bool? EnableSslVerification { get; init; }

    /// <summary>Whether a secret token is configured. The token itself is never returned.</summary>
    public bool? TokenPresent { get; init; }

    /// <summary>Whether an HMAC signing token is configured. The token itself is never returned.</summary>
    public bool? SigningTokenPresent { get; init; }

    /// <summary>Custom request-payload template, when the hook does not send GitLab's default body.</summary>
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