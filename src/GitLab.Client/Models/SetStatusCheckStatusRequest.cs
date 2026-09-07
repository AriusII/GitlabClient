namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/merge_requests/:iid/status_check_responses</c> - the callback an
///     external gate posts to report its verdict.
/// </summary>
public sealed record SetStatusCheckStatusRequest
{
    /// <summary>The <see cref="GitLabExternalStatusCheck.Id" /> of the check reporting in.</summary>
    public required long ExternalStatusCheckId { get; init; }

    /// <summary>The SHA at the HEAD of the merge request's source branch that this verdict applies to.</summary>
    public required string Sha { get; init; }

    /// <summary>One of <c>passed</c>, <c>failed</c> or <c>pending</c>.</summary>
    public required string Status { get; init; }
}