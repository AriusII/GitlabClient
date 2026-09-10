namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /projects/:id/merge_requests/:merge_request_iid/blocks</c>: which merge request
///     must be merged before this one.
/// </summary>
/// <remarks>
///     Identify the blocker either by its database <see cref="BlockingMergeRequestId" />, or by the
///     <see cref="BlockingMergeRequestIid" /> plus <see cref="BlockingProjectId" /> pair - an iid alone is
///     only unique within a project.
/// </remarks>
public sealed record CreateMergeRequestDependencyRequest
{
    /// <summary>The blocking merge request's database id.</summary>
    public long? BlockingMergeRequestId { get; init; }

    /// <summary>The blocking merge request's project-scoped iid. Pair it with <see cref="BlockingProjectId" />.</summary>
    public long? BlockingMergeRequestIid { get; init; }

    /// <summary>The project the blocking merge request lives in.</summary>
    public long? BlockingProjectId { get; init; }
}