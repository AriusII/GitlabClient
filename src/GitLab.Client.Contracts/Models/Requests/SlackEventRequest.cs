using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     The Slack Events API callback body GitLab accepts at <c>POST /integrations/slack/events</c>.
///     <para>
///         This endpoint exists to be called by Slack, not by an API consumer: Slack posts it on URL
///         verification and on every subscribed workspace event. It is wrapped for completeness and for
///         local testing of a self-managed instance's Slack app wiring. Every member is optional - the
///         spec declares no required properties, and the URL-verification handshake sends a different
///         subset than an <c>event_callback</c> does.
///     </para>
/// </summary>
public sealed record SlackEventRequest
{
    /// <summary>Slack's legacy verification token. Deprecated by Slack and unused by GitLab.</summary>
    public string? Token { get; init; }

    /// <summary>The Slack workspace the event happened in.</summary>
    public string? TeamId { get; init; }

    /// <summary>The Slack app the event was delivered to.</summary>
    public string? ApiAppId { get; init; }

    /// <summary>
    ///     The event itself. Its members differ for every Slack event type, so it is carried as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape the spec does not describe.
    /// </summary>
    public JsonElement? Event { get; init; }

    /// <summary>The envelope kind - usually <c>event_callback</c>, or <c>url_verification</c> on setup.</summary>
    public string? Type { get; init; }

    /// <summary>A unique id for this delivery, which Slack reuses when it retries.</summary>
    public string? EventId { get; init; }

    /// <summary>When Slack dispatched the event, as a Unix timestamp in seconds.</summary>
    public long? EventTime { get; init; }

    /// <summary>Slack user ids the event was authorized for. Deprecated by Slack.</summary>
    public IReadOnlyList<string>? AuthedUsers { get; init; }
}