namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /hooks</c> - registering an instance-level system hook. Requires instance
///     administrator rights.
/// </summary>
public sealed record CreateSystemHookRequest
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

    public bool? TagPushEvents { get; init; }

    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Fires when a repository's refs change outside a normal push (imports, mirrors, force updates).</summary>
    public bool? RepositoryUpdateEvents { get; init; }

    public bool? EnableSslVerification { get; init; }

    /// <summary>
    ///     Secret token GitLab sends as <c>X-Gitlab-Token</c> so the receiver can authenticate the delivery.
    ///     Write-only: it is never present on <see cref="GitLabSystemHook" />, which reports only
    ///     <see cref="GitLabSystemHook.TokenPresent" />.
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