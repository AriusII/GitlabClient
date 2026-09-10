namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/boards/:board_id</c>. Every field is optional; unset fields are
///     omitted from the payload rather than sent as null, so a partial update leaves the rest of the board
///     untouched.
/// </summary>
public sealed record UpdateBoardRequest
{
    public string? Name { get; init; }

    public bool? HideBacklogList { get; init; }

    public bool? HideClosedList { get; init; }

    /// <summary>Scopes the board to a single assignee (GitLab Premium).</summary>
    public long? AssigneeId { get; init; }

    /// <summary>Scopes the board to a milestone (GitLab Premium).</summary>
    public long? MilestoneId { get; init; }

    /// <summary>
    ///     Comma-separated list of label <b>names</b> to scope the board to (GitLab Premium). GitLab declares
    ///     this parameter as a single string, not an array, so it is not projected from a collection here.
    /// </summary>
    public string? Labels { get; init; }

    /// <summary>Scopes the board to an issue weight (GitLab Premium).</summary>
    public int? Weight { get; init; }
}