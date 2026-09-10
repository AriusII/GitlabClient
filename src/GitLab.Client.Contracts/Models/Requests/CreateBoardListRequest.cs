namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/boards/:board_id/lists</c>.
///     <para>
///         <b>Set exactly one of these four properties.</b> The spec marks none of them required, but GitLab
///         rejects a body carrying none - or more than one - with a 400 (surfaced here as
///         <c>GitLabValidationException</c>). The type system cannot express "exactly one of", so this is a
///         runtime contract the caller has to honour.
///     </para>
/// </summary>
public sealed record CreateBoardListRequest
{
    /// <summary>Creates a label list scoped to an existing project or group label.</summary>
    public long? LabelId { get; init; }

    /// <summary>Creates a milestone list (GitLab Premium).</summary>
    public long? MilestoneId { get; init; }

    /// <summary>Creates an iteration list (GitLab Premium).</summary>
    public long? IterationId { get; init; }

    /// <summary>Creates an assignee list (GitLab Premium).</summary>
    public long? AssigneeId { get; init; }
}