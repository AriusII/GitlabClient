namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>PUT /admin/active_context/collections/:id</c>, which updates one collection's
///     queue-sharding options. Every field is optional; omitted fields leave the current value unchanged.
/// </summary>
public sealed record UpdateActiveContextCollectionRequest
{
    public int? QueueShardCount { get; init; }

    public int? QueueShardLimit { get; init; }

    /// <summary>Defaults to the active connection when omitted.</summary>
    public long? ConnectionId { get; init; }
}