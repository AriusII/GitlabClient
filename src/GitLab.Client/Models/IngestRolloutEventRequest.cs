namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /rollouts/:id</c> - one flow-graph progress event reported by the CD
///     orchestrator (a Starlark workflow run by AutoFlow). Every member is optional: GitLab accepts the
///     call with an empty body.
/// </summary>
public sealed record IngestRolloutEventRequest
{
    /// <summary>The event topic.</summary>
    public string? Topic { get; init; }

    /// <summary>The event type.</summary>
    public string? Type { get; init; }

    /// <summary>The event payload.</summary>
    public RolloutEventPayload? Data { get; init; }

    /// <summary>AutoFlow Value (protojson), decoded into the same topic/type/data shape as <see cref="Data" />.</summary>
    public RolloutEventPayload? Value { get; init; }

    /// <summary>AutoFlow channels opened alongside this event, for posting a value back into them later.</summary>
    public IReadOnlyList<RolloutChannelToken>? ChannelTokens { get; init; }
}