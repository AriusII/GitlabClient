namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/hooks/:hook_id</c>.
///     <para>
///         Every member is nullable, including <see cref="Url" />: unset properties are omitted from the
///         payload rather than sent as null, so an update carries only what actually changes and leaves the
///         rest of the hook's configuration alone.
///     </para>
/// </summary>
public sealed record UpdateProjectHookRequest
{
    /// <summary>The endpoint GitLab posts to. May contain <c>{name}</c> placeholders filled from <see cref="UrlVariables" />.</summary>
    public Uri? Url { get; init; }

    /// <summary>Optional display name for the hook.</summary>
    public string? Name { get; init; }

    /// <summary>Optional free-text description of what the hook is for.</summary>
    public string? Description { get; init; }

    public bool? PushEvents { get; init; }

    /// <summary>Restricts <see cref="PushEvents" /> to branches matching this filter.</summary>
    public string? PushEventsBranchFilter { get; init; }

    /// <summary>How <see cref="PushEventsBranchFilter" /> is interpreted.</summary>
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

    /// <summary>Fires when a project access token is about to expire.</summary>
    public bool? ResourceAccessTokenEvents { get; init; }

    /// <summary>Fires when a deploy token is about to expire.</summary>
    public bool? ResourceDeployTokenEvents { get; init; }

    /// <summary>Whether Duo Agent Platform flows may post lifecycle events to this webhook.</summary>
    public bool? DuoFlowCallbackEnabled { get; init; }

    public bool? EnableSslVerification { get; init; }

    /// <summary>Replacement secret token. Write-only; GitLab never returns it.</summary>
    public string? Token { get; init; }

    /// <summary>Replacement HMAC signing key, in <c>whsec_&lt;base64&gt;</c> form. Write-only.</summary>
    public string? SigningToken { get; init; }

    /// <summary>Custom request-payload template, replacing the default GitLab body.</summary>
    public string? CustomWebhookTemplate { get; init; }

    /// <summary>
    ///     Replaces the hook's URL variables wholesale. To change a single one without resending the others,
    ///     use the dedicated <c>url_variables/:key</c> endpoint instead.
    /// </summary>
    public IReadOnlyList<GitLabHookUrlVariable>? UrlVariables { get; init; }

    /// <summary>Replaces the hook's custom headers wholesale.</summary>
    public IReadOnlyList<GitLabHookCustomHeader>? CustomHeaders { get; init; }
}