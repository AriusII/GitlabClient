namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>POST /projects/:id/merge_requests/:merge_request_iid/add_spent_time</c>.</summary>
public sealed record MergeRequestSpentTimeRequest
{
    /// <summary>
    ///     The time to log, in GitLab's human duration format - "3h30m". A negative duration ("-30m")
    ///     subtracts from the running total.
    /// </summary>
    public required string Duration { get; init; }

    /// <summary>An optional note recorded alongside the time entry.</summary>
    public string? Summary { get; init; }
}