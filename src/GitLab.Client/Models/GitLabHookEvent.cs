using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One delivery attempt recorded against a webhook
///     (<c>GET /projects/:id/hooks/:hook_id/events</c>, <c>GET /groups/:id/hooks/:hook_id/events</c>) -
///     what GitLab sent, what came back, and how long it took.
///     <para>
///         GitLab keeps these logs for seven days, so an empty result means "nothing recently", not
///         "nothing ever". Every member except <see cref="Id" /> is nullable: the spec declares no response
///         schema for this endpoint at all, and which fields are populated depends on how far the delivery
///         got - a request that never connected has no response headers, status or body.
///     </para>
/// </summary>
public sealed record GitLabHookEvent
{
    /// <summary>The hook log entry id, which is what <c>events/:hook_log_id/resend</c> takes.</summary>
    public required long Id { get; init; }

    /// <summary>The URL the delivery was sent to, with URL variables masked back to <c>{name}</c>.</summary>
    public Uri? Url { get; init; }

    /// <summary>The event that caused the delivery, in GitLab's hook naming - <c>push_hooks</c>, <c>issue_hooks</c>.</summary>
    public string? Trigger { get; init; }

    /// <summary>Headers GitLab sent, with the secret token redacted.</summary>
    public IReadOnlyDictionary<string, string>? RequestHeaders { get; init; }

    /// <summary>
    ///     The payload GitLab sent. Its shape depends entirely on <see cref="Trigger" /> (and on
    ///     <c>custom_webhook_template</c> when one is configured), so it is surfaced as a raw
    ///     <see cref="JsonElement" /> rather than forced into a type GitLab does not promise.
    /// </summary>
    public JsonElement? RequestData { get; init; }

    /// <summary>Headers the receiver answered with.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; init; }

    /// <summary>The receiver's response body, truncated by GitLab when large.</summary>
    public string? ResponseBody { get; init; }

    /// <summary>
    ///     The HTTP status the receiver answered with. A string rather than an int: GitLab writes the
    ///     transport-level failure here too (for example <c>internal error</c>) when there was no response.
    /// </summary>
    public string? ResponseStatus { get; init; }

    /// <summary>How long the delivery took, in seconds.</summary>
    public double? ExecutionDuration { get; init; }

    /// <summary>Why the delivery never reached the receiver - DNS, TLS or timeout failures land here.</summary>
    public string? InternalErrorMessage { get; init; }
}