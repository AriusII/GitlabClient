namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /groups/:id/hooks</c>.
///     <para>
///         Every trigger flag is nullable so an unset one is omitted from the payload and GitLab applies
///         its own default rather than being told <c>false</c>.
///     </para>
/// </summary>
public sealed record CreateGroupHookRequest
{
    /// <summary>The endpoint GitLab posts to. May contain <c>{name}</c> placeholders filled from <see cref="UrlVariables" />.</summary>
    public required Uri Url { get; init; }

    /// <summary>Optional display name for the hook.</summary>
    public string? Name { get; init; }

    /// <summary>Optional free-text description of what the hook is for.</summary>
    public string? Description { get; init; }

    public bool? PushEvents { get; init; }

    /// <summary>Restricts <see cref="PushEvents" /> to branches matching this filter.</summary>
    public string? PushEventsBranchFilter { get; init; }

    /// <summary>How <see cref="PushEventsBranchFilter" /> is interpreted. Defaults to wildcard matching.</summary>
    public GitLabHookBranchFilterStrategy? BranchFilterStrategy { get; init; }

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

    public bool? VulnerabilityEvents { get; init; }

    /// <summary>Fires when a subgroup is created or removed. Group hooks only.</summary>
    public bool? SubgroupEvents { get; init; }

    /// <summary>Fires when a project in the group is created, renamed, transferred or removed. Group hooks only.</summary>
    public bool? ProjectEvents { get; init; }

    /// <summary>Fires when group membership changes. Group hooks only.</summary>
    public bool? MemberEvents { get; init; }

    /// <summary>Fires when a group access token is about to expire.</summary>
    public bool? ResourceAccessTokenEvents { get; init; }

    /// <summary>Whether Duo Agent Platform flows may post lifecycle events to this webhook.</summary>
    public bool? DuoFlowCallbackEnabled { get; init; }

    public bool? EnableSslVerification { get; init; }

    /// <summary>
    ///     Secret token GitLab sends as <c>X-Gitlab-Token</c> so the receiver can authenticate the delivery.
    ///     Write-only: it is never present on <see cref="GitLabGroupHook" />, which reports only
    ///     <see cref="GitLabGroupHook.TokenPresent" />.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    ///     HMAC signing key used to compute the <c>webhook-signature</c> header, in <c>whsec_&lt;base64&gt;</c>
    ///     form encoding a 32-byte key. Write-only, like <see cref="Token" />.
    /// </summary>
    public string? SigningToken { get; init; }

    /// <summary>Custom request-payload template, replacing the default GitLab body.</summary>
    public string? CustomWebhookTemplate { get; init; }

    /// <summary>Values interpolated into <see cref="Url" />, so a secret never has to appear in the readable URL.</summary>
    public IReadOnlyList<GitLabHookUrlVariable>? UrlVariables { get; init; }

    /// <summary>Extra HTTP headers sent with every delivery.</summary>
    public IReadOnlyList<GitLabHookCustomHeader>? CustomHeaders { get; init; }
}