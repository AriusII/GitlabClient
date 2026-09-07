namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/environments/:environment_id/stop</c>. A one-field type so the
///     force-stop overload can reuse the ordinary create-with-body transport; the plain stop needs no body
///     at all.
/// </summary>
public sealed record StopEnvironmentRequest
{
    /// <summary>Stop the environment even when it has no stop action defined.</summary>
    public required bool Force { get; init; }
}