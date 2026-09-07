namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/housekeeping</c>.</summary>
public sealed record ProjectHousekeepingRequest
{
    /// <summary>
    ///     Which task to run. Left unset, GitLab picks the housekeeping task the project is due for.
    /// </summary>
    public GitLabHousekeepingTask? Task { get; init; }
}