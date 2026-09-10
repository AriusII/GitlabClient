namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>POST /admin/active_context/dead_queue/replay</c>, which enqueues a background task
///     that replays dead-lettered items into another queue.
/// </summary>
public sealed record ReplayActiveContextDeadQueueRequest
{
    /// <summary>
    ///     The target queue name - for example <c>retry_queue</c>, <c>second_retry_queue</c>, <c>code</c>,
    ///     <c>code_backfill</c>.
    /// </summary>
    public required string Queue { get; init; }
}