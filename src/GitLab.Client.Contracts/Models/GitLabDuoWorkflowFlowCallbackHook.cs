namespace GitLab.Client.Models;

/// <summary>
///     One registered Duo flow callback endpoint (<c>/ai/duo_workflows/flow_callbacks</c>) - an HTTPS URL
///     GitLab POSTs flow lifecycle events to, so a client does not have to poll a running flow.
///     <para>
///         Secrets are write-only: neither the signing token nor the shared token is ever returned, only
///         the two booleans saying whether each one is set.
///     </para>
/// </summary>
public sealed record GitLabDuoWorkflowFlowCallbackHook
{
    /// <summary>The registration id, which is what <c>callback_hook_id</c> on a flow refers to.</summary>
    public required long Id { get; init; }

    /// <summary>The HTTPS endpoint deliveries are sent to.</summary>
    public Uri? Url { get; init; }

    /// <summary>The label the endpoint was registered under.</summary>
    public string? Name { get; init; }

    /// <summary>Whether an HMAC signing secret is configured for this endpoint.</summary>
    public bool? SigningTokenSet { get; init; }

    /// <summary>Whether a shared secret sent as <c>X-Gitlab-Token</c> is configured for this endpoint.</summary>
    public bool? TokenSet { get; init; }

    /// <summary>When the endpoint was registered.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}