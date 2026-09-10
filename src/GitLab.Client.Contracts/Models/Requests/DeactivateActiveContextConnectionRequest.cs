namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>PUT /admin/active_context/connections/deactivate</c>. Deactivating a connection
///     asynchronously drops its indexed data and deletes the connection record.
/// </summary>
public sealed record DeactivateActiveContextConnectionRequest
{
    /// <summary>Defaults to the currently active connection when omitted.</summary>
    public long? ConnectionId { get; init; }
}