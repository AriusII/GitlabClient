namespace GitLab.Client.Models;

/// <summary>The body of <c>POST /usage_data/track_events</c> - a batch of internal GitLab events to record.</summary>
public sealed record TrackEventsRequest
{
    /// <summary>The events to record. GitLab allows at most 50 per request.</summary>
    public required IReadOnlyList<TrackEventRequest> Events { get; init; }
}