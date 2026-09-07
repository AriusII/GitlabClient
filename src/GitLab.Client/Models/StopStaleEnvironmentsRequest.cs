namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/environments/stop_stale</c>. GitLab requires the cut-off
///     date, so it is the type's only member.
/// </summary>
public sealed record StopStaleEnvironmentsRequest
{
    /// <summary>
    ///     Stop every environment last modified or deployed to before this moment. Protected environments
    ///     are excluded.
    /// </summary>
    public required DateTimeOffset Before { get; init; }
}