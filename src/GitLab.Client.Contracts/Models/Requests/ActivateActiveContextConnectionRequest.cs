namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>PUT /admin/active_context/connections/activate</c>. Activating this connection
///     deactivates whichever connection was previously active.
/// </summary>
public sealed record ActivateActiveContextConnectionRequest
{
    public required long ConnectionId { get; init; }
}