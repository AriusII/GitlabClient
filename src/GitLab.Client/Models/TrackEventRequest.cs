using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One internal GitLab event to record, as taken by <c>POST /usage_data/track_event</c> and, as the
///     element type of <see cref="TrackEventsRequest.Events" />, by <c>POST /usage_data/track_events</c>.
/// </summary>
public sealed record TrackEventRequest
{
    /// <summary>The event's registered name, e.g. <c>i_code_review_merge_request_widget_view</c>.</summary>
    public required string Event { get; init; }

    public long? NamespaceId { get; init; }

    public long? ProjectId { get; init; }

    public string? ProjectPath { get; init; }

    /// <summary>
    ///     Free-form key/value context sent alongside the event. GitLab's OpenAPI document types this as
    ///     a bare <c>object</c> whose keys are caller-defined, so it is kept as raw
    ///     <see cref="JsonElement" /> values rather than a guessed shape.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? AdditionalProperties { get; init; }

    /// <summary>Whether to also forward the event to Snowplow.</summary>
    public bool? SendToSnowplow { get; init; }
}